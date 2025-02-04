using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using NUnit.Framework;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Data.XAdES;
using SignatureVerifier.Security.Cryptography.Xml;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXmlTests
	{
		[Test]
		[TestCase(SignatureSourceType.Prescription)]
		[TestCase(SignatureSourceType.DispPrescription)]
		[TestCase(SignatureSourceType.Dispensing)]
		public void GetReferences(SignatureSourceType sourceType)
		{
			XmlDocument doc = null;
			XmlNamespaceManager nsManager = null;
			XmlNode sigNode = null;
			string[] refIdList = null;

			switch (sourceType) {
				case SignatureSourceType.Prescription:
					doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);
					nsManager = CreateNSManager(doc);
					sigNode = doc.SelectSingleNode("/Document/Prescription/PrescriptionSign/xs:Signature", nsManager);
					refIdList = new string[] { "#PrescriptionDocument", "#id6a875f31-KeyInfo", "#idc51bfd03-SignedProperties" };
					break;
				case SignatureSourceType.DispPrescription:
					doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);
					nsManager = CreateNSManager(doc);
					sigNode = doc.SelectSingleNode("/Document/Dispensing/ReferencePrescription/PrescriptionSign/xs:Signature", nsManager);
					refIdList = new string[] { "#PrescriptionDocument", "#id6a875f31-KeyInfo", "#idc51bfd03-SignedProperties" };
					break;
				case SignatureSourceType.Dispensing:
					doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);
					nsManager = CreateNSManager(doc);
					sigNode = doc.SelectSingleNode("/Document/DispensingSign/xs:Signature", nsManager);
					refIdList = new string[] { "#Dispensing", "#id771d57b8-KeyInfo", "#id111a1e5c-SignedProperties" };
					break;
			}

			var signature = new SignatureXml(sigNode, doc, nsManager);

			var references = signature.ReferenceValidationData;

			Assert.Multiple(() =>
			{
				Assert.That(signature.SourceType, Is.EqualTo(sourceType));
				Assert.That(references.Count(), Is.EqualTo(3));
			});

			foreach (var reference in signature.ReferenceValidationData) {
				Assert.That(reference.Uri, Is.AnyOf(refIdList));
				TestContext.WriteLine($"Reference Index:{reference.DispIndex}");
				TestContext.WriteLine($"Reference Transform:{reference.Transform}");
			}
		}

		[Test(Description = "TransformChainを使ってReferenceのハッシュ値を計算する")]
		public void TransformChainCalculatedDigestTest()
		{
			var doc = TestData.CreateXmlDocument(Resources.Europe);
			var nsManager = CreateNSManager(doc);

			var refNodes = doc.SelectNodes(".//xs:Reference", nsManager).OfType<XmlNode>();
			foreach (var refNode in refNodes) {

				var uri = refNode.GetUri();
				var digestValue = refNode.SelectSingleNode("xs:DigestValue", nsManager)?.InnerText;
				var digestAlgorithm = refNode.SelectSingleNode("xs:DigestMethod", nsManager)?.GetAlgorithm();

				var realNamespaces = ((XmlElement)refNode).GetAllNamespaces();
				var chain = new TransformChain();
				chain.LoadXml(refNode.SelectSingleNode("xs:Transforms", nsManager) as XmlElement);
				chain.AddNamespaces(realNamespaces);

				byte[] targetValue;
				if (string.IsNullOrEmpty(uri)) {
					targetValue = chain.GetOutput(doc).ToByteArray();
				}
				else {
					targetValue = chain.GetOutput(doc, uri).ToByteArray();
				}

				var calculatedValue = targetValue.CalculateDigest(digestAlgorithm.ToDerObjectIdentifier()).ToBase64String();

				TestContext.WriteLine($"ID:{refNode.GetId()}");
				TestContext.WriteLine($"URI:{uri}");
				TestContext.WriteLine($"digestValue:{digestValue}");
				TestContext.WriteLine($"calculatedValue:{calculatedValue}");

				Assert.Multiple(() =>
				{
					Assert.That(calculatedValue, Is.EqualTo(digestValue));
				});
			}
		}

		private static XmlNamespaceManager CreateNSManager(XmlDocument doc)
		{
			var nsManager = new XmlNamespaceManager(doc.NameTable);
			nsManager.AddNamespace("xs", "http://www.w3.org/2000/09/xmldsig#");
			nsManager.AddNamespace("xa", "http://uri.etsi.org/01903/v1.3.2#");
			nsManager.AddNamespace("xa141", "http://uri.etsi.org/01903/v1.4.1#");

			return nsManager;
		}
	}
}
