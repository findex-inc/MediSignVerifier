using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class KeyInfoXmlNodeExtensions
	{
		public static IEnumerable<XmlNode> GetX509CertificateNodes(this XmlNode keyInfo, XmlNamespaceManager nsManager)
		{
			return keyInfo.SelectNodes("xs:X509Data/xs:X509Certificate", nsManager).OfType<XmlNode>();
		}


		public static IEnumerable<XmlNode> GetX509IssuerSerialNodes(this XmlNode keyInfo, XmlNamespaceManager nsManager)
		{
			return keyInfo.SelectNodes("xs:X509Data/xs:X509IssuerSerial", nsManager).OfType<XmlNode>();
		}

	}
}
