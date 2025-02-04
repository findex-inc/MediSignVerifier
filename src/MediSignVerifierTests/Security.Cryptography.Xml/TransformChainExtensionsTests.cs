using System.Security.Cryptography.Xml;
using System.Xml;
using NUnit.Framework;

using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	[TestFixture]
	internal class TransformChainExtensionsTests
	{
		XmlDocument _document;
		TransformChain _transformChain;

		[SetUp]
		public void SetUp()
		{
			_document = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			_transformChain = new TransformChain();
			_transformChain.Add(SecurityCryptoXmlHelper.CreateTransform(SignedXml.XmlDsigC14NTransformUrl));
		}

		[TestCase("#id00fac23c-SignatureValue")]
		public void GetOutput(string uri)
		{
			using (var stream = _transformChain.GetOutput(_document, uri)) {
				Assert.That(stream, Is.Not.Null);
			}
		}
	}
}
