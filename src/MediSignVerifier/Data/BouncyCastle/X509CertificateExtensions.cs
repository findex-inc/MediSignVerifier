using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data.BouncyCastle.Asn1.HPKI;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class X509CertificateExtensions
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public static bool IsSelfSigned(this X509Certificate cert)
		{
			if (cert.IssuerDN.Equivalent(cert.SubjectDN)) {

				try {

					cert.Verify(cert.GetPublicKey());

					return true;
				}
				catch (Exception e) {

					_logger.Debug($"IssuerDN={cert.IssuerDN}, SerialNumber={cert.SerialNumber}");
					_logger.Debug(e, "Ignore exceptions.");
				}
			}

			return false;
		}

		public static byte[] CalculateDigest(this X509Certificate cert, DerObjectIdentifier algorithm)
		{
			return cert.GetEncoded().CalculateDigest(algorithm);
		}


		public static IEnumerable<HcRole> GetHcRoles(this X509Certificate cert)
		{
			var extensionValue = cert.GetExtensionValue(X509Extensions.SubjectDirectoryAttributes);
			if (extensionValue == null) {

				return Enumerable.Empty<HcRole>();
			}

			return ToHcRoles(extensionValue.GetOctets());

			IEnumerable<HcRole> ToHcRoles(byte[] rawData)
			{
				var sequence = Asn1Sequence.GetInstance(rawData);
				var subjectDirectoryAttributes = SubjectDirectoryAttributes.GetInstance(sequence);

				var hcRoleAttributes = subjectDirectoryAttributes.Attributes.OfType<AttributeX509>()
					.Where(x => HPKIIdentifier.id_hcpki_at_healthcareactor.Equals(x.AttrType));

				var hcActorDatas = hcRoleAttributes.SelectMany(x => x.AttrValues.OfType<Asn1Set>());

				var hcActors = hcActorDatas
					.SelectMany(hcActorData => hcActorData.OfType<Asn1Sequence>().Select(x => HCActor.GetInstance(x)));

				var hcRoleWrappers = hcActors
					.Where(x => HPKIIdentifier.id_jhpki_cdata.Equals(x.CodedData.CodingSchemeReference))
					.Select(x => new HcRole(
						x.CodedData.CodingSchemeReference.Id,
						x.CodedData.CodeDataFreeText.ToString()))
					.ToArray();

				return hcRoleWrappers;
			}
		}

		public static string DumpSubjectDirectoryAttributesAsString(this X509Certificate cert)
		{
			var extensionValue = cert.GetExtensionValue(X509Extensions.SubjectDirectoryAttributes);
			var sequence = Asn1Sequence.GetInstance(extensionValue.GetOctets());

			return Asn1Dump.DumpAsString(sequence);
		}

		public static IEnumerable<string> GetExtendedKeyUsageNames(this X509Certificate cert)
		{
			return cert?.GetExtendedKeyUsage()?.OfType<string>()
				.Select(oid => KeyPurposeNames.TryGetValue(oid, out var text) ? text : oid);
		}

		private static readonly IDictionary<string, string> KeyPurposeNames = new Dictionary<string, string>()
		{
			[KeyPurposeID.IdKPServerAuth.Id] = nameof(KeyPurposeID.IdKPServerAuth),
			[KeyPurposeID.IdKPClientAuth.Id] = nameof(KeyPurposeID.IdKPClientAuth),
			[KeyPurposeID.IdKPCodeSigning.Id] = nameof(KeyPurposeID.IdKPCodeSigning),
			[KeyPurposeID.IdKPEmailProtection.Id] = nameof(KeyPurposeID.IdKPEmailProtection),
			[KeyPurposeID.IdKPIpsecEndSystem.Id] = nameof(KeyPurposeID.IdKPIpsecEndSystem),
			[KeyPurposeID.IdKPIpsecTunnel.Id] = nameof(KeyPurposeID.IdKPIpsecTunnel),
			[KeyPurposeID.IdKPIpsecUser.Id] = nameof(KeyPurposeID.IdKPIpsecUser),
			[KeyPurposeID.IdKPTimeStamping.Id] = nameof(KeyPurposeID.IdKPTimeStamping),
			[KeyPurposeID.IdKPOcspSigning.Id] = nameof(KeyPurposeID.IdKPOcspSigning),
			[KeyPurposeID.IdKPSmartCardLogon.Id] = nameof(KeyPurposeID.IdKPSmartCardLogon),
			[KeyPurposeID.IdKPMacAddress.Id] = nameof(KeyPurposeID.IdKPMacAddress),

		};


		public static string Report(this X509Certificate cert)
		{
			var builder = new StringBuilder();
			builder.Append("{ ");
			builder.AppendFormat("Version={0}", cert.Version);
			builder.AppendFormat(", SerialNumber={0}", cert.SerialNumber);
			builder.AppendFormat(", IssuerDN={0}", cert.IssuerDN);
			builder.AppendFormat(", SubjectDN={0}", cert.SubjectDN);

#if DEBUG
			builder.AppendLine();
			builder.Append(cert.ToString());
#endif
			builder.Append(" }");
			builder.AppendLine();

			return builder.ToString();
		}
	}
}
