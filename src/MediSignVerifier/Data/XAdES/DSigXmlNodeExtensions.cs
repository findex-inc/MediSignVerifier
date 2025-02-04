using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class DSigXmlNodeExtensions
	{
		public static XmlNode GetX509IssuerNameNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:X509IssuerName", nsManager);
		}


		public static XmlNode GetX509SerialNumberNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:X509SerialNumber", nsManager);
		}


		public static XmlNode GetDigestMethodNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:DigestMethod", nsManager);
		}


		public static XmlNode GetDigestValueNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:DigestValue", nsManager);
		}

		public static XmlNode GetSignatureMethodNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:SignatureMethod", nsManager);
		}

		public static XmlNode GetCanonicalizationMethodNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:CanonicalizationMethod", nsManager);
		}

		public static XmlNode GetTransformsNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xs:Transforms", nsManager);
		}

		public static IEnumerable<XmlNode> GetTransformNodes(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectNodes("xs:Transforms/xs:Transform", nsManager).OfType<XmlNode>();
		}

		public static XmlNode GetXPathNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			//XPath要素が複数ある場合・・出てきたときに考える
			return parent.SelectSingleNode("xs:XPath", nsManager);
		}
	}
}
