using NUnit.Framework;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	[TestFixture]
	internal class SecurityCryptoXmlHelperTests
	{
		//リフレクションで取得したメソッド等がNullでないことを確認する

		[Test]
		public void GetProperty_ReferenceSignedXml()
		{
			var prop = SecurityCryptoXmlHelper.Reference_SignedXml;
			Assert.That(prop, Is.Not.Null);
		}

		[Test]
		public void GetMethod_CreateFromName()
		{
			var method = SecurityCryptoXmlHelper.CryptoHelpers_CreateFromName;
			Assert.That(method, Is.Not.Null);
		}

		[Test]
		public void GetMethod_CreateHashAlgorithmFormName()
		{
			var method = SecurityCryptoXmlHelper.CryptoHelpers_CreateHashAlgorithmFromName;
			Assert.That(method, Is.Not.Null);
		}

		[Test]
		public void GetMethod_CreateSignatureDescriptionFormName()
		{
			var method = SecurityCryptoXmlHelper.CryptoHelpers_CreateSignatureDescriptionFromName;
			Assert.That(method, Is.Not.Null);
		}

		[Test]
		public void GetMethod_CreateTransformFromName()
		{
			var method = SecurityCryptoXmlHelper.CryptoHelpers_CreateTransformFromName;
			Assert.That(method, Is.Not.Null);
		}

		[Test]
		public void GetConstructor_CanonicalXmlNodeList()
		{
			var ctor = SecurityCryptoXmlHelper.CanonicalXmlNodeList_Constructor;
			Assert.That(ctor, Is.Not.Null);
		}

		[Test]
		public void GetMethod_ReferenceCalculateHashValue()
		{
			var method = SecurityCryptoXmlHelper.Reference_CalculateHashValue;
			Assert.That(method, Is.Not.Null);
		}

		[TestCase("http://www.w3.org/TR/2001/REC-xml-c14n-20010315")]
		[TestCase("http://www.w3.org/2001/10/xml-exc-c14n#")]
		public void CreateTransform(string algorithm)
		{
			var transform = SecurityCryptoXmlHelper.CreateTransform(algorithm);
			Assert.That(transform, Is.Not.Null);
		}

		[TestCase("http://www.w3.org/2001/10/xml-exc-c15n#")]
		[TestCase(null)]
		[TestCase("")]
		public void CreateTransformError(string algorithm)
		{
			var transform = SecurityCryptoXmlHelper.CreateTransform(algorithm);
			Assert.That(transform, Is.Null);
		}

		[TestCase("http://www.w3.org/2001/04/xmlenc#sha256")]
		[TestCase("http://www.w3.org/2001/04/xmlenc#sha512")]
		public void IsSupportedHashAlgorithm(string algorithm)
		{
			var result = SecurityCryptoXmlHelper.IsSupportedHashAlgorithm(algorithm);
			Assert.That(result, Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("http://www.w3.org/2001/10/xml-exc-c14n#")]
		[TestCase("http://www.w3.org/2000/09/xmldsig#")]
		public void IsSupportedHashAlgorithmError(string algorithm)
		{
			var result = SecurityCryptoXmlHelper.IsSupportedHashAlgorithm(algorithm);
			Assert.That(result, Is.False);
		}

		[TestCase("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")]
		public void IsSupportedSignatureAlgorithm(string algorithm)
		{
			var result = SecurityCryptoXmlHelper.IsSupportedSignatureAlgorithm(algorithm);
			Assert.That(result, Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("http://www.w3.org/2001/04/xmlenc#sha512")]
		[TestCase("http://www.w3.org/2001/10/xml-exc-c14n#")]
		public void IsSupportedSignatureAlgorithmError(string algorithm)
		{
			var result = SecurityCryptoXmlHelper.IsSupportedSignatureAlgorithm(algorithm);
			Assert.That(result, Is.False);
		}
	}
}
