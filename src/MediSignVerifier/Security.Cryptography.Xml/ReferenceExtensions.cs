using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	internal static class ReferenceExtensions
	{

		public static void SetSignedXml(this Reference reference, SignedXml xml)
		{
			SecurityCryptoXmlHelper.Reference_SignedXml.SetValue(reference, xml, null);
		}

		public static void AddTransform(this Reference reference, string algorithm)
		{
			reference.AddTransform(SecurityCryptoXmlHelper.CreateTransform(algorithm));
		}

		public static byte[] CalculateDigest(this Reference reference, XmlDocument baseDocument, IEnumerable<XmlAttribute> namespaces)
		{
			reference.TransformChain.AddNamespaces(namespaces);
			var refList = SecurityCryptoXmlHelper.CanonicalXmlNodeList_Constructor.Invoke(null);
			return (byte[])SecurityCryptoXmlHelper.Reference_CalculateHashValue.Invoke(reference, new object[] { baseDocument, refList });
		}
	}
}
