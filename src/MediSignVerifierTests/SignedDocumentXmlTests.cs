using System.Linq;
using System.Xml;
using NUnit.Framework;
using MediSignVerifier.Tests.Properties;

namespace SignatureVerifier
{
	[TestFixture]
	internal partial class SignedDocumentXmlTests
	{
		XmlDocument _prescriptionDoc;
		XmlDocument _dispensingDoc;

		[SetUp]
		public void Setup()
		{
			_prescriptionDoc = TestData.CreateXmlDocument(Resources.Prescription_004_01);
			_dispensingDoc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);
		}

		[Test]
		public void ConstructorPrescription()
		{
			var psData = new SignedDocumentXml(_prescriptionDoc, "Prescription-004_01.xml");

			Assert.Multiple(() =>
			{
				Assert.That(psData.DocumentType, Is.EqualTo(DocumentType.Prescription));
				Assert.That(psData.Raw, Is.SameAs(_prescriptionDoc));
				Assert.That(psData.Signatures.Count(), Is.EqualTo(1));
			});
		}

		[Test]
		public void ConstructorDispensing()
		{
			var psData = new SignedDocumentXml(_dispensingDoc, "Dispensing-005_03.xml");

			Assert.Multiple(() =>
			{
				Assert.That(psData.DocumentType, Is.EqualTo(DocumentType.Dispensing));
				Assert.That(psData.Raw, Is.SameAs(_dispensingDoc));
				Assert.That(psData.Signatures.Count(), Is.EqualTo(2));
			});
		}
	}
}
