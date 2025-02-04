using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class UnsignedSignaturePropertiesXmlNodeExtensions
	{
		public static IEnumerable<XmlNode> GetCertificateValuesNodes(this XmlNode unsignedSignatureProperties, XmlNamespaceManager nsManager)
		{
			return unsignedSignatureProperties.SelectNodes("xa:CertificateValues/xa:EncapsulatedX509Certificate", nsManager).OfType<XmlNode>();
		}


		public static IEnumerable<XmlNode> GetCRLValuesNodes(this XmlNode unsignedSignatureProperties, XmlNamespaceManager nsManager)
		{
			return unsignedSignatureProperties.SelectNodes("xa:RevocationValues/xa:CRLValues/xa:EncapsulatedCRLValue", nsManager).OfType<XmlNode>();
		}


		public static IEnumerable<XmlNode> GetOCSPValuesNodes(this XmlNode unsignedSignatureProperties, XmlNamespaceManager nsManager)
		{
			return unsignedSignatureProperties.SelectNodes("xa:RevocationValues/xa:OCSPValues/xa:EncapsulatedOCSPValue", nsManager).OfType<XmlNode>();
		}

	}
}
