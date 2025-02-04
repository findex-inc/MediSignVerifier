using System.Linq;
using System.Reflection;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class TimeStampTokenExtensions
	{
		public static SignerInformation GetSignerInfo(this TimeStampToken token)
		{
			var prop = typeof(TimeStampToken).GetField("tsaSignerInfo", BindingFlags.NonPublic | BindingFlags.Instance);
			return (SignerInformation)prop.GetValue(token);
		}

		public static TimeStampCertID GetCertID(this TimeStampToken token)
		{
			Org.BouncyCastle.Asn1.Cms.Attribute attr = token.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificate]
				?? token.SignedAttributes[PkcsObjectIdentifiers.IdAASigningCertificateV2];

			if (attr != null) {
				if (attr.AttrValues[0] is SigningCertificateV2) {
					var signCert = SigningCertificateV2.GetInstance(attr.AttrValues[0]);
					var certId = EssCertIDv2.GetInstance(signCert.GetCerts()[0]);
					return new TimeStampCertID(certId.HashAlgorithm, certId.GetCertHash(), certId.IssuerSerial);
				}
				else {
					//※ESSCertIDはSHA1のみ。
					var signCert = SigningCertificate.GetInstance(attr.AttrValues[0]);
					var certId = EssCertID.GetInstance(signCert.GetCerts()[0]);
					return new TimeStampCertID(new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1), certId.GetCertHash(), certId.IssuerSerial);
				}
			}

			return null;
		}

		public static X509Certificate GetSignerCertificate(this TimeStampToken token)
		{
			return token.GetCertificates().GetMatches(null).OfType<X509CertificateStructure>()
				.Where(x => x.Issuer.Equivalent(token.SignerID.Issuer))
				.Select(x => new X509Certificate(x)).FirstOrDefault();
		}

		public static byte[] GetMessageDigest(this TimeStampToken token)
		{
			Org.BouncyCastle.Asn1.Cms.Attribute attr = token.SignedAttributes[PkcsObjectIdentifiers.Pkcs9AtMessageDigest];
			return attr?.AttrValues[0] is Asn1OctetString ? ((Asn1OctetString)attr.AttrValues[0]).GetOctets() : null;
		}


		public static string DumpAsString(this TimeStampToken tat)
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

			string ReportAccuracy(Accuracy acc)
			{
				if (acc == null) return null;

				return $"{acc.Seconds}s {acc.Millis}ms {acc.Micros}us";
			}

			string ReportCertificate(X509CertificateStructure cert)
			{
				if (cert == null) return null;

				return new X509Certificate(cert).ToString();
			}

			var cms = tat?.ToCmsSignedData();

			AppendLine("Content");
			AppendLine("  CMSVersion", $"{cms?.Version}");

			AppendLine("  EncapsulatedContentInfo");
			AppendLine("    eContentType", $"{cms?.SignedContentType}");
			AppendLine("    eContent");
			AppendLine("      Version", $"{tat?.TimeStampInfo.TstInfo.Version}");
			AppendLine("      Policy", $"{tat?.TimeStampInfo.Policy}");
			AppendLine("      MessageImprint");
			AppendLine("        hashAlgorithm[OID]", $"{tat?.TimeStampInfo.MessageImprintAlgOid}");
			AppendLine("        hashedMessage", $"{tat?.TimeStampInfo.GetMessageImprintDigest()?.ToBase64String()}");
			AppendLine("      serialNumber", $"{tat?.TimeStampInfo.SerialNumber}");
			AppendLine("      genTime", $"{tat?.TimeStampInfo.GenTime}");

			AppendLine("      Accuracy", $"{ReportAccuracy(tat?.TimeStampInfo.Accuracy)} ");
			AppendLine("      Ordering", $"{tat?.TimeStampInfo.TstInfo.Ordering}");
			AppendLine("      Nonce", $"{tat?.TimeStampInfo.Nonce}");
			AppendLine("      Tsa", $"{tat?.TimeStampInfo.Tsa}");
			AppendLine("      Extensions", $"{tat?.TimeStampInfo.TstInfo.Extensions}");

			AppendLine("  CertificateSet(Certificates)");
			var certificates = tat?.GetCertificates();
			if (certificates != null) {
				foreach (var set in certificates.GetMatches(null).OfType<X509CertificateStructure>()
					.Select((x, i) => new { Cert = x, Index = i })) {
					AppendLine($"    [{set.Index}]", $"{ReportCertificate(set.Cert)}");
				}
			}

			AppendLine("  RevocationInfoChoices(crls)");
			var revocations = tat?.GetCrls("Collection");
			if (revocations != null) {
				foreach (var set in revocations.GetMatches(null).OfType<X509Crl>()
					.Select((x, i) => new { Crl = x, Index = i })) {
					AppendLine($"    [{set.Index}]", $"{set.Crl}");
				}
			}

			var signerInfo = tat?.ToCmsSignedData().GetSignerInfos().GetSigners().OfType<SignerInformation>().First();

			AppendLine("  SignerInfos");
			AppendLine("    SignerIdentifier");
			AppendLine("      IssuerAndSerialNumber");
			AppendLine("        Issuer", $"{tat?.SignerID.Issuer.ToString()}");
			AppendLine("        SerialNumber", $"{tat?.SignerID.SerialNumber}");
			AppendLine("      SubjectKeyIdentifier", $"{tat?.SignerID.SubjectKeyIdentifier}");
			AppendLine("    DigestAlgorithmIdentifier", $"{signerInfo?.DigestAlgOid}");

			AppendLine("    SignedAttributes");
			var signedAttributes = tat?.SignedAttributes?.ToAttributes();
			if (signedAttributes != null) {

				foreach (var set in signedAttributes.GetAttributes()
					.Select((x, i) => new { Attr = x, Index = i })) {

					AppendLine($"      [{set.Index}]",
						$"{{ OID:\"{set.Attr.AttrType}\", Value: {set.Attr.AttrValues} }}");
				}
			}
			AppendLine("    SignatureAlgorithm", $"{signerInfo?.EncryptionAlgOid}");
			AppendLine("    SignatureValue", $"{signerInfo?.GetSignature().ToBase64String()}");

			AppendLine("    UnsignedAttributes");
			var unsignedAttributes = tat?.UnsignedAttributes?.ToAttributes();
			if (unsignedAttributes != null) {

				foreach (var set in unsignedAttributes.GetAttributes()
					.Select((x, i) => new { Attr = x, Index = i })) {

					AppendLine($"      [{set.Index}]",
						$"{{ OID:\"{set.Attr.AttrType}\", Value: {set.Attr.AttrValues} }}");
				}
			}

			return builder.ToString();
		}
	}


	internal static class TimeStampTokenInfoExtensions
	{
		public static DerObjectIdentifier GetMessageImprintAlgorithm(this TimeStampTokenInfo info)
		{
			return info.HashAlgorithm.Algorithm;
		}
	}

	internal class TimeStampCertID
	{
		public TimeStampCertID(AlgorithmIdentifier hashAlgorithmID, byte[] certHash, IssuerSerial issuerSerial)
		{
			HashAlgorithmID = hashAlgorithmID;
			CertHash = certHash;
			IssuerSerial = issuerSerial;
		}

		public AlgorithmIdentifier HashAlgorithmID { get; }

		public DerObjectIdentifier HashAlgorithm => HashAlgorithmID.Algorithm;

		public byte[] CertHash { get; }

		public IssuerSerial IssuerSerial { get; }
	}

}
