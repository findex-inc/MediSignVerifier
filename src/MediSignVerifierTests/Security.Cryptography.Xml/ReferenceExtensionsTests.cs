using System;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using NUnit.Framework;

using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	[TestFixture]
	internal class ReferenceExtensionsTests
	{
		XmlNamespaceManager _nsManager;
		XmlDocument _document;
		Reference _reference;

		[SetUp]
		public void Setup()
		{
			_document = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			_nsManager = new XmlNamespaceManager(_document.NameTable);
			_nsManager.AddNamespace("xs", "http://www.w3.org/2000/09/xmldsig#");
			_nsManager.AddNamespace("xa", "http://uri.etsi.org/01903/v1.3.2#");
			_nsManager.AddNamespace("xa141", "http://uri.etsi.org/01903/v1.4.1#");

			//とりあえず1個目のReferenceを対象にテスト
			var refNode = _document.SelectSingleNode("/Document/Prescription/PrescriptionSign/xs:Signature/xs:SignedInfo/xs:Reference", _nsManager) as XmlElement;

			var uri = refNode.GetAttribute("URI");
			_reference = new Reference(uri);
			_reference.LoadXml(refNode);
		}

		[TestCase("http://www.w3.org/2001/10/xml-exc-c14n#")]
		public void AddTransform(string algorithm)
		{
			_reference.AddTransform(algorithm);
			Assert.That(_reference.TransformChain.Count, Is.GreaterThan(0));
		}

		[TestCase(null)]
		[TestCase("")]
		public void AddTransformError(string algorithm)
		{
			var ex = Assert.Throws<ArgumentNullException>(() => _reference.AddTransform(algorithm));
			Assert.That(ex, Is.Not.Null);
		}

		[Test]
		public void CalculateDigestAndEqualToReferenceDigest()
		{
			_reference.SetSignedXml(new SignedXml(_document));

			var calcDigest = _reference.CalculateDigest(_document, Enumerable.Empty<XmlAttribute>());
			Assert.That(calcDigest, Is.Not.Null);
			Assert.That(calcDigest, Is.EqualTo(_reference.DigestValue));
		}

	}
}
