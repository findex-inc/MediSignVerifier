using System.Linq;
using System.Xml;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Data.XAdES
{
	internal class SigningCertificateV2ValidationData : SigningCertificateValidationData
	{
		public SigningCertificateV2ValidationData(XmlNode signingCertificate, XmlNode properties, XmlNamespaceManager nsManager)
			: base(signingCertificate, properties, nsManager)
		{
		}


		protected override CertificateIssuerSerialData CreateCertificateIssuerSerialData(XmlNode signingCertificate, XmlNamespaceManager nsManage)
		{
			var issuerSerialElem = signingCertificate.GetIssuerSerialV2Node(nsManage);
			if (issuerSerialElem != null) {

				var issurSerial = issuerSerialElem.InnerText?.ToBytes()?.ToX509IssuerSerial();

				var id = signingCertificate?.GetAncestorId();
				var issuerName = issurSerial?.Issuer.GetNames().Select(x => x.Name.ToString()).FirstOrDefault();
				var serialNumber = issurSerial?.Serial.Value.ToString(16).PaddingHex();

				return new CertificateIssuerSerialData(id, issuerName, serialNumber);
			}

			return null;
		}
	}
}
