using System.Xml;
using NUnit.Framework;

namespace SignatureVerifier.Data.XAdES
{
	internal class XmlNodeExtensionsTests
	{

		[Test]
		[TestCase(Case1forGetAbsolutePath, "/Document/Prescription/PrescriptionSign/xs:Signature")]
		[TestCase(Case2forGetAbsolutePath, "/Document/DispensingSign/xs:Signature")]
		[TestCase(Case3forGetAbsolutePath, "/Document/Dispensing/ReferencePrescription/PrescriptionSign/xs:Signature")]
		public void GetAbsolutePath(string xml, string expected)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			var actual = node.GetAbsolutePath();

			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		[TestCase(Case1forGetAbsolutePath, "/Document/Prescription/PrescriptionSign/xs:Signature[@Id=\"PrescriptionSign\"]")]
		[TestCase(Case2forGetAbsolutePath, "/Document/DispensingSign/xs:Signature[@Id=\"DispensingSign\"]")]
		[TestCase(Case3forGetAbsolutePath, "/Document/Dispensing/ReferencePrescription/PrescriptionSign/xs:Signature[@Id=\"PrescriptionSign\"]")]
		public void GetAbsolutePath_AdditionalId(string xml, string expected)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			var actual = node.GetAbsolutePath(additionalId: true);

			Assert.That(actual, Is.EqualTo(expected));
		}




		private const string Case1forGetAbsolutePath = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""Document""
  xsi:noNamespaceSchemaLocation=""EP.xsd"">
  <Prescription>
    <PrescriptionManagement id=""PrescriptionManagement"">
      <Version Value=""EPS1.0"" />
    </PrescriptionManagement>
    <PrescriptionDocument id=""PrescriptionDocument"">
      <!-- test for SigningCertificateSignatureXmlTests. -->
    </PrescriptionDocument>
    <PrescriptionSign>
      <xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""PrescriptionSign"">
      </xs:Signature>
    </PrescriptionSign>
  </Prescription>
</Document>
";
		private const string Case2forGetAbsolutePath = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""Document""
	xsi:noNamespaceSchemaLocation=""ED.xsd"">
	<DispensingManagement id=""DispensingManagement"">
	</DispensingManagement>
	<Dispensing id=""Dispensing"">
	</Dispensing>
	<DispensingSign>
		<xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""DispensingSign"">
		</xs:Signature>
	</DispensingSign>
</Document>
";
		private const string Case3forGetAbsolutePath = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""Document""
	xsi:noNamespaceSchemaLocation=""ED.xsd"">
	<Dispensing id=""Dispensing"">
		<ReferencePrescription>
			<PrescriptionSign>
				<xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""PrescriptionSign"">
				</xs:Signature>
			</PrescriptionSign>
		</ReferencePrescription>
	</Dispensing>
</Document>	
";
	}
}
