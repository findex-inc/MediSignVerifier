using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class SigningCertificateXmlNodeExtensions
	{
		public static XmlNode GetCertDigestNode(this XmlNode signingCertificate, XmlNamespaceManager nsManager)
		{
			return signingCertificate.SelectSingleNode("xa:Cert/xa:CertDigest", nsManager);
		}


		public static XmlNode GetIssuerSerialNode(this XmlNode signingCertificate, XmlNamespaceManager nsManager)
		{
			return signingCertificate.SelectSingleNode("xa:Cert/xa:IssuerSerial", nsManager);
		}


		public static XmlNode GetIssuerSerialV2Node(this XmlNode signingCertificateV2, XmlNamespaceManager nsManager)
		{
			return signingCertificateV2.SelectSingleNode("xa:Cert/xa:IssuerSerialV2", nsManager);
		}

	}
}
