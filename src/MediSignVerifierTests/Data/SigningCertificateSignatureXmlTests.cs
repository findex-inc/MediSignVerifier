using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SignatureVerifier.Data.XAdES;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Data
{
	public class SigningCertificateSignatureXmlTests
	{
		[Test(Description = "プロパティ値は、必ず同一インスタンスを返却すること。")]
		public void SigningCertificateValidationData_ReturnSameInstance()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundAtX509Certificate);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual1st = signature.SigningCertificateValidationData;
			var actual2nd = signature.SigningCertificateValidationData;
			var actual3rd = signature.SigningCertificateValidationData;

			// Assert.
			Assert.Multiple(() =>
			{
				Assert.That(object.ReferenceEquals(actual1st, actual2nd), Is.True);
				Assert.That(object.ReferenceEquals(actual2nd, actual3rd), Is.True);
				Assert.That(object.ReferenceEquals(actual3rd, actual1st), Is.True);
			});

			return;
		}

		[Test(Description = "Found at //KeyInfo/X509Data/X509Certificate")]
		public void SigningCertificateValidationData_WithFoundAtX509Certificate()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundAtX509Certificate);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.InstanceOf<KeyInfoSigningCertificateValidationData>());

			Assert.Multiple(() =>
			{
				Assert.That(actual.Referenced, Is.EqualTo("KeyInfo"));
				Assert.That(actual.Certificate, Is.Not.Null);
				Assert.That(actual.CertDigest, Is.Null);
				Assert.That(actual.IssuerSerial, Is.Null);
				Assert.That(actual.PathValidationData, Is.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.Certificate.Id, Is.EqualTo("idTEST0001-KeyInfo"));
				Assert.That(actual.Certificate.Value, Is.Not.Null);
			});

			return;
		}


		[Test(Description = "Found at //KeyInfo/X509Data/X509IssuerSerial")]
		public void SigningCertificateValidationData_WithFoundAtX509IssuerSerial()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundAtX509IssuerSerial);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.InstanceOf<KeyInfoSigningCertificateValidationData>());

			Assert.Multiple(() =>
			{
				Assert.That(actual.Referenced, Is.EqualTo("KeyInfo"));
				Assert.That(actual.Certificate, Is.Not.Null);
				Assert.That(actual.CertDigest, Is.Null);
				Assert.That(actual.IssuerSerial, Is.Not.Null);
				Assert.That(actual.PathValidationData, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.Certificate.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.Certificate.Id, Is.EqualTo("xa:CertificateValues/xa:EncapsulatedX509Certificate[1]"));
				Assert.That(actual.Certificate.Value, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.IssuerSerial.Id, Is.EqualTo("idTEST0002-KeyInfo"));
				Assert.That(actual.IssuerSerial.IssuerName, Is.EqualTo("C=JP,O=MEDIS,OU=MEDIS HPKI CA,CN=HPKI-01-MedisSignCA2-forNonRepudiation"));
				Assert.That(actual.IssuerSerial.SerialNumber, Is.EqualTo("015e"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.PathValidationData.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.PathValidationData.Certificates.Count(), Is.EqualTo(2));
				Assert.That(actual.PathValidationData.Crls, Is.Empty);
				Assert.That(actual.PathValidationData.Ocsps, Is.Empty);
			});

			return;
		}


		public enum NotFoundKeyInfoType
		{
			UnmatchReferenceKeyInfo,
			UnsupportedKeyInfo,
		}

		[Test(Description = "Not Found //KeyInfo")]
		public void SigningCertificateValidationData_WithNotFoundKeyInfo([Values] NotFoundKeyInfoType type)
		{
			// Arrange.
			var doc = new XmlDocument();

			switch (type) {
				case NotFoundKeyInfoType.UnmatchReferenceKeyInfo:
					doc.LoadXml(Resources.UnmatchReferenceKeyInfo);
					break;
				case NotFoundKeyInfoType.UnsupportedKeyInfo:
					doc.LoadXml(Resources.UnsupportedKeyInfo);
					break;
				default:
					throw new InvalidOperationException($"Invalid test type.");
			}

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.Null);

		}

		[Test(Description = "Found at //SignedSignatureProperties/SigningCertificateV2")]
		public void SigningCertificateValidationData_WithFoundAtSigningCertificateV2()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundAtSigningCertificateV2);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.InstanceOf<SigningCertificateValidationData>());

			Assert.Multiple(() =>
			{
				Assert.That(actual.Referenced, Is.EqualTo("SigningCertificateV2"));
				Assert.That(actual.Certificate, Is.Not.Null);
				Assert.That(actual.CertDigest, Is.Not.Null);
				Assert.That(actual.IssuerSerial, Is.Not.Null);
				Assert.That(actual.PathValidationData, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.Certificate.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.Certificate.Id, Is.EqualTo("xa:CertificateValues/xa:EncapsulatedX509Certificate[1]"));
				Assert.That(actual.Certificate.Value, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.CertDigest.Id, Is.EqualTo("idTEST0005-SignedProperties"));
				Assert.That(actual.CertDigest.DigestMethod, Is.EqualTo("http://www.w3.org/2001/04/xmlenc#sha256"));
				Assert.That(actual.CertDigest.DigestValue, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.IssuerSerial.Id, Is.EqualTo("idTEST0005-SignedProperties"));
				Assert.That(actual.IssuerSerial.IssuerName, Is.EqualTo("C=JP,O=MEDIS,OU=MEDIS HPKI CA,CN=HPKI-01-MedisSignCA2-forNonRepudiation"));
				Assert.That(actual.IssuerSerial.SerialNumber, Is.EqualTo("015e"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.PathValidationData.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.PathValidationData.Certificates.Count(), Is.EqualTo(2));
				Assert.That(actual.PathValidationData.Crls, Is.Empty);
				Assert.That(actual.PathValidationData.Ocsps, Is.Empty);
			});

			return;
		}

		[Test(Description = "Found at //SignedSignatureProperties/SigningCertificate")]
		public void SigningCertificateValidationData_WithFoundAtSigningCertificate()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundAtSigningCertificate);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.InstanceOf<SigningCertificateValidationData>());

			Assert.Multiple(() =>
			{
				Assert.That(actual.Referenced, Is.EqualTo("SigningCertificate"));
				Assert.That(actual.Certificate, Is.Not.Null);
				Assert.That(actual.CertDigest, Is.Not.Null);
				Assert.That(actual.IssuerSerial, Is.Not.Null);
				Assert.That(actual.PathValidationData, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.Certificate.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.Certificate.Id, Is.EqualTo("xa:CertificateValues/xa:EncapsulatedX509Certificate[1]"));
				Assert.That(actual.Certificate.Value, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.CertDigest.Id, Is.EqualTo("idTEST0006-SignedProperties"));
				Assert.That(actual.CertDigest.DigestMethod, Is.EqualTo("http://www.w3.org/2001/04/xmlenc#sha256"));
				Assert.That(actual.CertDigest.DigestValue, Is.Not.Null);
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.IssuerSerial.Id, Is.EqualTo("idTEST0006-SignedProperties"));
				Assert.That(actual.IssuerSerial.IssuerName, Is.EqualTo("C=JP,O=MEDIS,OU=MEDIS HPKI CA,CN=HPKI-01-MedisSignCA2-forNonRepudiation"));
				Assert.That(actual.IssuerSerial.SerialNumber, Is.EqualTo("015e"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(actual.PathValidationData.Source, Is.EqualTo("/Document/Prescription/PrescriptionSign/xs:Signature/xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties"));
				Assert.That(actual.PathValidationData.Certificates.Count(), Is.EqualTo(2));
				Assert.That(actual.PathValidationData.Crls, Is.Empty);
				Assert.That(actual.PathValidationData.Ocsps, Is.Empty);
			});

			return;
		}



		[Test(Description = "Does not exist anywhere.")]
		public void SigningCertificateValidationData_WithNotFoundAnywhere()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.NotFoundAnywhere);

			var nsManager = doc.CreateXAdESNamespaceManager();

			var node = doc.SelectSingleNode("//xs:Signature", nsManager);

			ISignature signature = new SignatureXml(node, doc, nsManager);

			// Act.
			var actual = signature.SigningCertificateValidationData;

			// Assert.
			Assert.That(actual, Is.Null);
		}

	}
}
