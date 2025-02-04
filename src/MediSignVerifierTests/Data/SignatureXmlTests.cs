using System.Xml;
using NUnit.Framework;
using SignatureVerifier.Data.XAdES;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXmlTests
	{
		[Test]
		public void ESLevel_WithNormalpatterns([Values] ESLevel level)
		{
			// Arrange.
			var doc = new XmlDocument();
			switch (level) {
				case ESLevel.BES:
					doc.LoadXml(@"<root><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"</xs:Signature></root>");
					break;
				case ESLevel.T:
					doc.LoadXml(@"<root><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"<xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties><xa:UnsignedProperties><xa:UnsignedSignatureProperties>" +
						@"<xa:SignatureTimeStamp />" +
						@"</xa:UnsignedSignatureProperties></xa:UnsignedProperties></xa:QualifyingProperties></xs:Object>" +
						@"</xs:Signature></root>");
					break;
				case ESLevel.XL:
					doc.LoadXml(@"<root><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"<xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties><xa:UnsignedProperties><xa:UnsignedSignatureProperties>" +
						@"<xa:SignatureTimeStamp />" +
						@"<xa:CertificateValues />" +
						@"</xa:UnsignedSignatureProperties></xa:UnsignedProperties></xa:QualifyingProperties></xs:Object>" +
						@"</xs:Signature></root>");
					break;
				case ESLevel.A:
					doc.LoadXml(@"<root><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"<xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties><xa:UnsignedProperties><xa:UnsignedSignatureProperties>" +
						@"<xa:SignatureTimeStamp />" +
						@"<xa:CertificateValues />" +
						@"<xa141:ArchiveTimeStamp xmlns:xa141=""http://uri.etsi.org/01903/v1.4.1#"" />" +
						@"</xa:UnsignedSignatureProperties></xa:UnsignedProperties></xa:QualifyingProperties></xs:Object>" +
						@"</xs:Signature></root>");
					break;
				default:
					doc.LoadXml(@"<root></root>");
					break;
			}

			var nsManager = doc.CreateXAdESNamespaceManager();
			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act
			var actual = signature.ESLevel;

			// Assert.
			Assert.That(actual, Is.EqualTo(level));
		}

		[Test]
		public void SourceType_WithNormalPatterns([Values] SignatureSourceType type)
		{
			// Arrange.
			var doc = new XmlDocument();

			switch (type) {
				case SignatureSourceType.Prescription:
					doc.LoadXml(@"<Document ><Prescription><PrescriptionSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"</xs:Signature></PrescriptionSign></Prescription></Document >");
					break;
				case SignatureSourceType.Dispensing:
					doc.LoadXml(@"<Document ><DispensingSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"</xs:Signature></DispensingSign></Document >");
					break;
				case SignatureSourceType.DispPrescription:
					doc.LoadXml(@"<Document ><Dispensing><ReferencePrescription><PrescriptionSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"</xs:Signature></PrescriptionSign></ReferencePrescription></Dispensing></Document >");
					break;
				case SignatureSourceType.Unknown:
					doc.LoadXml(@"<xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" >" +
						@"</xs:Signature>");
					break;
				default:
					doc.LoadXml(@"<root></root>");
					break;
			}

			var nsManager = doc.CreateXAdESNamespaceManager();
			var node = doc.SelectSingleNode("//xs:Signature", nsManager);
			if (node != null) {
				ISignature signature = new SignatureXml(node, doc, nsManager);

				// Act
				var actual = signature.SourceType;

				// Assert.
				Assert.That(actual, Is.EqualTo(type));
			}
		}

	}
}
