using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class SignatureXmlNodeExtensions
	{
		public static ESLevel GetESLevel(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			if (signature == null) {
				return ESLevel.None;
			}

			if (!signature.IsEsLevelT(nsManager)) {
				return ESLevel.BES;
			}

			if (!signature.IsEsLevelXL(nsManager)) {
				return ESLevel.T;
			}

			if (!signature.IsEsLevelA(nsManager)) {
				return ESLevel.XL;
			}

			return ESLevel.A;
		}

		public static SignatureSourceType GetSignatureSourceType(this XmlNode signature)
		{
			if (signature.LocalName == "Signature") {
				switch (signature.GetAbsolutePath()) {
					case "/Document/Prescription/PrescriptionSign/xs:Signature":
						return SignatureSourceType.Prescription;
					case "/Document/DispensingSign/xs:Signature":
						return SignatureSourceType.Dispensing;
					case "/Document/Dispensing/ReferencePrescription/PrescriptionSign/xs:Signature":
						return SignatureSourceType.DispPrescription;
					default:
						return SignatureSourceType.Unknown;
				}
			}

			//Signature要素ではない
			return SignatureSourceType.None;
		}


		public static XmlNode FindUnsignedSignaturePropertiesNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode(".//xa:UnsignedSignatureProperties", nsManager);
		}


		public static XmlNode FindReferencedKeyInfoNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			var references = signature.SelectNodes(".//xs:Reference", nsManager).OfType<XmlNode>();

			var keyInfo = signature.SelectNodes(".//xs:KeyInfo", nsManager).OfType<XmlNode>()
				.Where(x => references.Any(n => n.IsReference(x.GetId())))
				.FirstOrDefault();

			return keyInfo;
		}


		public static XmlNode FindSigningCertificateNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode(".//xa:SigningCertificate", nsManager);
		}


		public static XmlNode FindSigningCertificateV2Node(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode(".//xa:SigningCertificateV2", nsManager);
		}

		public static XmlNode GetSignedInfoNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:SignedInfo", nsManager);
		}

		public static IEnumerable<XmlNode> GetSignedInfoReferenceNodes(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectNodes("xs:SignedInfo/xs:Reference", nsManager).OfType<XmlNode>();
		}

		public static XmlNode GetSignatureValueNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:SignatureValue", nsManager);
		}

		public static XmlNode GetKeyInfoNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:KeyInfo", nsManager);
		}

		public static IEnumerable<XmlNode> GetObjectNodes(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectNodes("xs:Object", nsManager).OfType<XmlNode>();
		}

		public static XmlNode GetSignatureTimeStampNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/" +
				"xa:SignatureTimeStamp", nsManager);
		}


		public static XmlNode GetCertificateValuesNode(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/" +
				"xa:CertificateValues", nsManager);
		}


		public static IEnumerable<XmlNode> GetArchiveTimeStampNodes(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			return signature.SelectNodes("xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/" +
				"xa141:ArchiveTimeStamp", nsManager).OfType<XmlNode>();
		}

		public static XmlNode GetTimeStampValidationDataNode(this XmlNode signature, string timeStampNodeId, XmlNamespaceManager nsManager)
		{
			return signature.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/" +
				$"xa141:TimeStampValidationData[@URI='#{timeStampNodeId}']", nsManager);
		}


		public static bool IsEsLevelT(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			var result = signature.GetSignatureTimeStampNode(nsManager) != null;

			return result;
		}


		public static bool IsEsLevelXL(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			var result = signature.GetCertificateValuesNode(nsManager) != null;

			return result;
		}


		public static bool IsEsLevelA(this XmlNode signature, XmlNamespaceManager nsManager)
		{
			var result = signature.GetArchiveTimeStampNodes(nsManager).Any();

			return result;
		}

	}
}
