using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class OcspRespExtensions
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public static T GetResponseObject<T>(this OcspResp ocspResp)
			where T : class
		{
			return ocspResp.GetResponseObject() as T;
		}

		public static X509Certificate GetSignedCert(this OcspResp ocspResp)
		{
			var basic = ocspResp.GetResponseObject<BasicOcspResp>();
			var signedCert = basic.GetSignedCert();

			return signedCert;
		}

		public static X509Certificate GetSignedCert(this BasicOcspResp basic)
		{
			return basic.GetCerts().FirstOrDefault();
		}


		public static OcspResp FindByCertificate(this IEnumerable<OcspResp> ocsps,
			X509Certificate cert,
			X509Certificate sign)
		{
			return ocsps.Where(x => x.IsMatch(cert, sign)).FirstOrDefault();
		}


		public static bool IsMatch(this OcspResp ocspResp,
			X509Certificate cert,
			X509Certificate sign
			)
		{
			if (ocspResp.Status != OcspResponseStatus.Successful) {
				return false;
			}

			var basic = ocspResp.GetResponseObject<BasicOcspResp>();
			var signedCert = basic.GetSignedCert();

			if (!basic.Verify(signedCert.GetPublicKey())) {
				return false;
			}

			AuthorityKeyIdentifier auth;
			try {
				auth = AuthorityKeyIdentifier.GetInstance(
					X509ExtensionUtilities.FromExtensionValue(
						cert.GetExtensionValue(X509Extensions.AuthorityKeyIdentifier)));
			}
			catch (Exception e) {
				throw new ApplicationException(
					"Authority key identifier extension could not be extracted from cert.", e);
			}
			var authorityKeyHash = auth?.GetKeyIdentifier();

			var certIssure = PrincipalUtilities.GetIssuerX509Principal(cert);
			var certSerialNumber = cert.SerialNumber;

			var issuerPublicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(sign.GetPublicKey())?.PublicKeyData;

			foreach (var response in basic.Responses) {
				var certId = response.GetCertID();
				var algorithm = certId.HashAlgOid;

				// Align with LeXAdES implementation.
				//_ = certId.MatchesIssuer(sign);

				var issuerKeyHash = certId.GetIssuerKeyHash();
				authorityKeyHash = authorityKeyHash
						?? DigestUtilities.CalculateDigest(algorithm, issuerPublicKey.GetBytes());
				if (!issuerKeyHash.SequenceEqual(authorityKeyHash)) {
					_logger.Debug("unmatch is ...\n" +
							$"\t ocsp value = {issuerKeyHash.ToBase64String()}\n" +
							$"\t cert value  = {authorityKeyHash.ToBase64String()}");
					continue;
				}

				var issuerNameHash = certId.GetIssuerNameHash();
				var calculatedIssuerHash = DigestUtilities.CalculateDigest(algorithm, certIssure.GetEncoded());
				if (!issuerNameHash.SequenceEqual(calculatedIssuerHash)) {
					_logger.Debug("unmatch is ...\n" +
							$"\t ocsp value = {issuerNameHash.ToBase64String()}\n" +
							$"\t cert value = {calculatedIssuerHash.ToBase64String()}");
					continue;
				}

				var serialNumber = certId.SerialNumber;
				if (!serialNumber.Equals(certSerialNumber)) {
					_logger.Debug("unmatch is ...\n" +
							$"\t ocsp value = {serialNumber}\n" +
							$"\t cert value = {certSerialNumber}");
					continue;
				}

				return true;
			}

			return false;
		}


		public static string DumpAsString(this OcspResp ocspResp)
		{
			var builder = new StringBuilder();

			void AppendLine(string key, string value = null)
			{
				const int spacing = -40;

				if (value == null) {
					builder.AppendLine(key);
				}
				else {
					builder.AppendLine($"{key,spacing} = {value}");
				}
			}

			string ReportCertificate(X509Certificate cert)
			{
				if (cert == null) return null;

				return cert.ToString();
			}

			var status = ocspResp.Status == OcspResponseStatus.Successful
					? "successful"
					: "";

			var basic = ocspResp.GetResponseObject<BasicOcspResp>();

			var responseType = basic != null
				? "id-pkix-ocsp-basic"
				: "id-pkix-ocsp?";

			AppendLine("OCSPResponse");
			AppendLine("  responseStatus", $"[{{ocspResp.Status}}] {status}");
			AppendLine("  responseBytes", "...");
			builder.AppendLine();

			AppendLine("ResponseBytes");
			AppendLine("  responseType", $"{responseType}");
			AppendLine("  response", $"...");
			builder.AppendLine();

			AppendLine("BasicOCSPResponse");
			AppendLine("  tbsResponseData");
			AppendLine("    version", $"{basic.Version}");
			AppendLine("    responderID");
			AppendLine("      byName", $"{basic.ResponderId?.ToAsn1Object().Name}");
			AppendLine("      byKey", $"{basic.ResponderId?.ToAsn1Object()?.GetKeyHash()?.ToBase64String()}");

			AppendLine("    producedAt", $"{basic.ProducedAt}");
			AppendLine("    responses");
			foreach (var response in basic.Responses
				.Select((x, i) => new { Resp = x, Index = i })) {

				var resp = response.Resp;
				var cid = resp.GetCertID();
				var revoked = resp.GetCertStatus() as RevokedStatus;

				AppendLine($"      [{response.Index}]");
				AppendLine("        certID");
				AppendLine("          hashAlgorithm", $"{cid?.HashAlgOid}");
				AppendLine("          issuerNameHash", $"{cid?.GetIssuerNameHash().ToBase64String()}");
				AppendLine("          issuerKeyHash", $"{cid?.GetIssuerKeyHash().ToBase64String()}");
				AppendLine("          serialNumber", $"{cid?.SerialNumber}");

				AppendLine("        certStatus", $"{resp.GetCertStatus() ?? " <good>"}");
				AppendLine("          revocationTime", $"{revoked?.RevocationTime}");
				AppendLine("          revocationReason", $"{revoked?.RevocationReason}");

				AppendLine("        thisUpdate", $"{resp.ThisUpdate}");
				AppendLine("        nextUpdate", $"{resp.NextUpdate}");
				AppendLine("        singleExtensions");

				foreach (var set in (resp.SingleExtensions?.GetExtensionOids()
						?? Enumerable.Empty<DerObjectIdentifier>())
						.Select((x, i) => new { Oid = x, Index = i })) {

					var oid = set.Oid;
					var ext = resp.SingleExtensions.GetExtension(oid);
					AppendLine($"          [{set.Index}]",
						$"{{ OID:\"{oid.Id}\", VALUE: {ext.Value} }}");
				}
			}

			AppendLine("    responseExtensions");
			foreach (var set in (basic.ResponseExtensions?.GetExtensionOids()
					?? Enumerable.Empty<DerObjectIdentifier>())
					.Select((x, i) => new { Oid = x, Index = i })) {

				var oid = set.Oid;
				var ext = basic.ResponseExtensions.GetExtension(oid);
				AppendLine($"          [{set.Index}]",
					$"{{ OID:\"{oid.Id}\", VALUE: {ext.Value} }}");
			}

			AppendLine("  signatureAlgorithm");
			AppendLine("    SignatureAlgName", $"{basic.SignatureAlgName}");
			AppendLine("    SignatureAlgOid", $"{basic.SignatureAlgOid}");

			AppendLine("  signature", $"{basic.GetSignature().ToBase64String()}");
			AppendLine("  certs");
			foreach (var set in basic.GetCerts()
				.Select((x, i) => new { Cert = x, Index = i })) {

				AppendLine($"    [{set.Index}]", $"{ReportCertificate(set.Cert)}");
			}
			builder.AppendLine();

			return builder.ToString();
		}

	}
}
