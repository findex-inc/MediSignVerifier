using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Verifiers.BouncyCastle.Pkix;

#pragma warning disable IDE0060

namespace SignatureVerifier.Verifiers
{
	internal static class CertificatePathValidationDataExtensions
	{
		public static void Validate(this CertificatePathValidationData validationData,
			DateTime validDate,
			CertificateData targetCert,
			VerificationConfig config,
			PkixCertPathChecker checker = null
			)
		{
			var endCert = GetEndEntityCertificate(targetCert);

			var certs = GetCertificates(validationData)
					.AddIfNotContaints(endCert);
			var crls = GetCrls(validationData);
			var ocsps = GetOcsps(validationData);

			var trusted = certs.Where(x => x.IsSelfSigned());
			if (!trusted.Any()) {
				throw new CertificatePathValidationException(
					VerificationStatus.INDETERMINATE, "トラストアンカーが見つかりません。");
			}

			var parameters = new PkixBuilderParametersBuilder(trusted,
				new X509CertStoreSelector()
				{
					Subject = endCert.SubjectDN
				})
				.SetDate(validDate)
				.AddStore(certs)
				.AddStore(crls)
				.AddPathChecker(checker)
				.Build();

			var builder = new PkixCertPathBuilder();
			var validator = new PkixCertPathValidator();
			var revokedValidator = new RevocationPkixCertPathValidator();

			try {
				parameters.IsRevocationEnabled = false;
				var built = builder.Build(parameters);

				var parameters2 = (PkixParameters)parameters.Clone();
				_ = validator.Validate(built.CertPath, parameters2);

				var parameters3 = (PkixParameters)parameters.Clone();
				parameters3.IsRevocationEnabled = true;
				_ = revokedValidator.Validate(built.CertPath, ocsps, parameters3);

			}
			catch (GeneralSecurityException ex) {

				throw CreateCertificatePathValidationException(ex);
			}

			return;
		}


		private static X509Certificate GetEndEntityCertificate(CertificateData targetData)
		{
			X509Certificate endCert;
			try {

				endCert = targetData.Value.ToX509Certificate();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is CertificateParsingException) {

				throw new CertificatePathValidationException(
					VerificationStatus.INVALID, "証明書の構造が正しくありません。", ex);
			}

			return endCert;
		}

		private static IEnumerable<X509Certificate> GetCertificates(CertificatePathValidationData validationData)
		{
			IEnumerable<X509Certificate> certs;
			try {

				certs = validationData?.Certificates
					?.Select(x => x.ToX509Certificate()).ToArray()
					?? Enumerable.Empty<X509Certificate>();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is CertificateParsingException) {

				throw new CertificatePathValidationException(
					VerificationStatus.INVALID, "証明書の構造が正しくありません。", ex);
			}

			return certs;
		}

		private static IEnumerable<X509Crl> GetCrls(CertificatePathValidationData validationData)
		{
			IEnumerable<X509Crl> crls;
			try {
				crls = validationData?.Crls
					?.Select(x => x.ToX509Crl()).ToArray()
					?? Enumerable.Empty<X509Crl>();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is CrlException) {

				throw new CertificatePathValidationException(
					VerificationStatus.INVALID, "失効情報(CRL)の構造が正しくありません。", ex);
			}

			return crls;
		}

		private static IEnumerable<OcspResp> GetOcsps(CertificatePathValidationData validationData)
		{
			IEnumerable<OcspResp> crls;
			try {
				crls = validationData?.Ocsps
					?.Select(x => x.ToOcspResp()).ToArray()
					?? Enumerable.Empty<OcspResp>();
			}
			catch (Exception ex) when (ex is ArgumentException) {

				throw new CertificatePathValidationException(
					VerificationStatus.INVALID, "失効情報(OCSP)の構造が正しくありません。", ex);
			}

			return crls;
		}


		private static CertificatePathValidationException CreateCertificatePathValidationException(GeneralSecurityException ex)
		{
			var message = ex.InnerException?.Message ?? ex.Message;

			// [検証要件] 5.7 証明書の検証要件 - 証明書パス構築 - 拡張領域における制約の確認
			{
				var pattterns = new[]
				{
					@"^IssuerDomainPolicy is anyPolicy$",
					@"^SubjectDomainPolicy is anyPolicy,$",
					@"^Not a CA certificate$",
					@"^Intermediate certificate lacks BasicConstraints$",
					@"^Max path length not greater than zero$",
					@"^Issuer certificate keyusage extension is critical and does not permit key signing.$",
					@"^Certificate has unsupported critical extension$",
				};

				foreach (var pattern in pattterns) {

					if (Regex.IsMatch(message, pattern)) {

						return new CertificatePathValidationException(
							VerificationStatus.INVALID, $"証明書の制約を満足していません。{message}", ex);
					}
				}
			}

			// [検証要件] 5.7 証明書の検証要件 - 証明書パス構築 - 証明書パス構築の確認

			// @"^No certificate found matching targetConstraints.$"

			if (Regex.IsMatch(message, @"^No issuer certificate for certificate in certification path found.$", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INDETERMINATE, "上位証明書が見つかりません。", ex);
			}

			if (Regex.IsMatch(message, @"^Unable to find certificate chain.$", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INDETERMINATE, "トラストアンカーにたどりつくことができません。", ex);
			}

			// [検証要件] 5.7 証明書の検証要件 - 証明書パス検証 - 証明書の改ざん確認

			if (Regex.IsMatch(message, @"^Could not validate certificate signature.$", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INVALID, "証明書の署名検証が失敗しました。", ex);
			}

			// [検証要件] 5.7 証明書の検証要件 - 証明書パス検証 - 失効確認

			if (Regex.IsMatch(message, @"^Certificate revocation ", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INVALID, $"証明書が失効しています。{message}", ex);
			}

			// [検証要件] 5.7 証明書の検証要件 - 証明書パス検証 - 失効確認

			if (Regex.IsMatch(message, @"^Could not validate certificate: ", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INVALID, $"証明書が有効期間内にありません。{message}", ex);
			}

			// [検証要件] 5.7 証明書の検証要件 - 失効情報 - 失効情報の妥当性確認

			if (Regex.IsMatch(message, @"^No (CRLs|OCSP) found for issuer ", RegexOptions.Compiled)) {

				return new CertificatePathValidationException(
					VerificationStatus.INDETERMINATE, $"失効情報が発行されていません。{message}", ex);
			}

			return new CertificatePathValidationException(
					VerificationStatus.INVALID, $"証明書のパス検証が失敗しました。{message}", ex);
		}

	}
}
