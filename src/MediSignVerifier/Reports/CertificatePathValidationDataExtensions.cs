using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Data.BouncyCastle.Pkix;
using SignatureVerifier.Reports.BouncyCastle;

namespace SignatureVerifier.Reports
{
	internal static class CertificatePathValidationDataExtensions
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


		public static IEnumerable<ICertificateInfo> ToReportCertificates(this CertificatePathValidationData validationData,
			DateTime validDate,
			CertificateData targetCert,
			ReportConfig config,
			PkixCertPathChecker checker = null)
		{
			var endCert = targetCert?.Value?.ToX509CertificateOrDefault();

			// decode error.
			if (endCert == null) {
				yield break;
			}

			var certs = (validationData.Certificates ?? Enumerable.Empty<byte[]>())
					.Select(x => x.ToX509CertificateOrDefault())
					.AddIfNotContaints(endCert)
					.Where(x => x != null).ToArray();

			var trusted = certs.Where(x => x.IsSelfSigned());

			// Report builds non-trust self-signed certificate as anchor.
			var selfSigned = certs.Where(x => x.IsSelfSigned());

			if (!selfSigned.Any()) {

				if (endCert != null) {

					var info = endCert.ToCertificateInfo(targetCert?.Source, validDate, revocations: null);

					yield return info;
				}

				yield break;
			}

			var crls = (validationData.Crls ?? Enumerable.Empty<byte[]>())
					.Select(x => x.ToX509CrlOrDefault()).Where(x => x != null).ToArray();
			var ocsps = (validationData.Ocsps ?? Enumerable.Empty<byte[]>())
					.Select(x => x.ToOcspRespOrDefault()).Where(x => x != null).ToArray();

			var parameters = new PkixBuilderParametersBuilder(selfSigned,
				new X509CertStoreSelector()
				{
					Subject = endCert.SubjectDN
				})
				.SetDate(validDate)
				.AddStore(certs)
				.AddStore(crls)
				.AddPathChecker(checker ?? new IgnoreErrorsChecker())
				.Build();

			var builder = new PkixCertPathBuilder();

			PkixCertPathBuilderResult built;
			try {

				parameters.IsRevocationEnabled = false;
				parameters.MaxPathLength = -1;

				built = builder.Build(parameters);

			}
			catch (PkixCertPathBuilderException e) {

				_logger.Debug(e, "Ignore exceptions, return null.");

				built = null;
			}

			// build error.
			if (built == null) {

				if (endCert != null) {

					var info = endCert.ToCertificateInfo(targetCert?.Source, validDate, revocations: null);

					yield return info;
				}

				yield break;
			}

			if (built.CertPath != null) {

				var param = (PkixParameters)parameters.Clone();
				var certificates = built.CertPath.Certificates.OfType<X509Certificate>().ToArray();

				foreach (var i in Enumerable.Range(0, certificates.Length)) {

					X509Certificate cert = certificates[i];
					X509Certificate sign = ((i + 1) < certificates.Length)
						? certificates[i + 1]
						: built.TrustAnchor.TrustedCert;

					var revocations = FindRevocations(validationData.Source, cert, sign, ocsps, param);

					var source = FindCertificateSource(cert, validationData, targetCert, config);
					var info = new X509CertificateInfo(source, cert, validDate, revocations);

					yield return info;
				}
			}

			if (built.TrustAnchor != null) {

				var cert = built.TrustAnchor.TrustedCert;
				var source = FindCertificateSource(cert, validationData, targetCert, config);
				var info = new X509CertificateInfo(source, cert, validDate, revocations: null);

				yield return info;
			}

		}


		private static IEnumerable<ICertificateRevocationInfo> FindRevocations(
			string source, X509Certificate cert, X509Certificate sign, OcspResp[] ocsps, PkixParameters param)
		{
			var ocsp = ocsps.FindByCertificate(cert, sign);
			if (ocsp != null) {

				yield return new OCSPRevocationInfo(source, cert, ocsp);
				yield break;
			}

			IEnumerable<X509Crl> crls;
			try {

				crls = PkixCertPathUtilities.FindCrls(cert, param);
			}
			catch (ApplicationException e) {

				_logger.Debug(e, "Ignore exceptions, return empty.");

				yield break;
			}

			foreach (var crl in crls) {

				yield return new X509CRLRevocationInfo(source, cert, crl);
			}

		}


		private static string FindCertificateSource(X509Certificate cert,
			CertificatePathValidationData validationData,
			CertificateData targetCert,
			ReportConfig _)
		{
			var endCert = targetCert?.Value?.ToX509CertificateOrDefault();

			if (cert.Equals(endCert)) {

				return targetCert?.Source;
			}

			var other = validationData.Certificates
					?.Where(x => cert.Equals(x.ToX509CertificateOrDefault()))
					.FirstOrDefault();

			if (other != null) {

				return validationData.Source;
			}

			return "<unknown>";
		}


		// Do not use it for purposes other than reporting.
		// レポート以外の目的で使用しないでください。

		private class IgnoreErrorsChecker : PkixCertPathChecker
		{
			public override void Init(bool forward)
			{
			}

			public override bool IsForwardCheckingSupported()
			{
				return false;
			}

			public override ISet GetSupportedExtensions()
			{
				return null;
			}

			public override void Check(X509Certificate cert, ISet unresolvedCritExts)
			{
				unresolvedCritExts?.Clear();
				return;
			}

		}

	}
}
