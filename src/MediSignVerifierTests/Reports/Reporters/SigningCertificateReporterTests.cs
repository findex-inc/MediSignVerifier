using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.TestUtilities;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SigningCertificateReporterTests
	{
		private static readonly CertificatePathValidationDataGenerator _certPathDataGenerator = new CertificatePathValidationDataGenerator();

		[Test]
		public void Generate_Normal()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var before = utcDateTime.AddSeconds(-50);
			var after = utcDateTime.AddSeconds(50);

			var (eeCert, pathData, trust) = _certPathDataGenerator.GetTestData(utcDateTime, "id-test-certificate-001", caLength: 3);

			var config = new ReportConfig();

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.SetupGet(x => x.Certificate).Returns(eeCert);
			validationData.SetupGet(x => x.PathValidationData).Returns(pathData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.DispPrescription);
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.T);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var result = new VerificationResult(VerificationStatus.VALID, new[]
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription,
					"source-prescription-1", "item-prescription-1", "mapped-prescription-1", "message-prescription-1"),
				new VerificationResultItem(VerificationStatus.INDETERMINATE, SignatureSourceType.Prescription,
					"source-prescription-2", "item-prescription-2", "mapped-prescription-2", "message-prescription-2"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription,
					"source-prescription-3", "item-prescription-3", "mapped-prescription-3", "message-prescription-3"),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing,
					"source-dispensing-1", "item-dispensing-1", "mapped-dispensing-1", "message-dispensing-1"),
				new VerificationResultItem(VerificationStatus.INDETERMINATE, SignatureSourceType.Dispensing,
					"source-dispensing-2", "item-dispensing-2", "mapped-dispensing-2", "message-dispensing-2"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Dispensing,
					"source-dispensing-3", "item-dispensing-3", "mapped-dispensing-3", "message-dispensing-3"),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription,
					"source-reference-1", "item-reference-1", "mapped-reference-1", "message-reference-1"),
				new VerificationResultItem(VerificationStatus.INDETERMINATE, SignatureSourceType.DispPrescription,
					"source-reference-2", "item-reference-2", "mapped-reference-2", "message-reference-2"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.DispPrescription,
					"source-reference-3", "item-reference-3", "mapped-reference-3", "message-reference-3"),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.None,
					"source-none-1", "item-none-1", "mapped-none-1", "message-none-1"),
				new VerificationResultItem(VerificationStatus.INDETERMINATE, SignatureSourceType.None,
					"source-none-2", "item-none-2", "mapped-none-2", "message-none-2"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.None,
					"source-none-3", "item-none-3", "mapped-none-3", "message-none-3"),
			});

			var target = new SigningCertificateReporter(config);

			//Act.
			var actual = target.Generate(signature.Object, utcDateTime, result);

			//Assert
			TestContext.WriteLine(TestData.ToJson(actual, DateTimeZoneHandling.RoundtripKind));

			signature.Verify();
			validationData.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Id, Is.EqualTo("id-test-certificate-001"));
				Assert.That(actual.CertificateBaseTime, Is.EqualTo(utcDateTime));
				Assert.That(actual.Certificates, Has.Length.EqualTo(5));
				Assert.That(actual.ResultItems, Has.Length.EqualTo(3));
			});

			Assert.Multiple(() =>
			{
				var cert = actual.Certificates.ElementAt(0);

				Assert.That(cert.SerialNumber, Is.EqualTo("008000000000000000"));
				Assert.That(cert.Issuer, Is.EqualTo("CN=No.102 Test Intermediate CA Certificate"));
				Assert.That(cert.Subject, Is.EqualTo("CN=Test End Entity Certificate"));
				Assert.That(cert.NotBefore, Is.EqualTo(before));
				Assert.That(cert.NotAfter, Is.EqualTo(after));
				Assert.That(cert.IsCa, Is.Null);
				Assert.That(cert.HcRole, Is.EqualTo("Medical Tester"));

				Assert.That(cert.ExtendedKeyUsage, Has.Length.EqualTo(3));
				Assert.That(cert.ExtendedKeyUsage[0], Is.EqualTo("IdKPServerAuth"));
				Assert.That(cert.ExtendedKeyUsage[1], Is.EqualTo("IdKPIpsecUser"));
				Assert.That(cert.ExtendedKeyUsage[2], Is.EqualTo("IdKPTimeStamping"));

				Assert.That(cert.Revocations, Has.Length.EqualTo(1));
				var revoke = cert.Revocations.First();
				Assert.That(revoke.DataType, Is.EqualTo(CertificateRevocationDataType.CRL));
				Assert.That(revoke.Issuer, Is.EqualTo("CN=No.102 Test Intermediate CA Certificate"));
				Assert.That(revoke.ThisUpdate, Is.EqualTo(utcDateTime));
				Assert.That(revoke.NextUpdate, Is.EqualTo(utcDateTime.AddSeconds(100)));
				Assert.That(revoke.Entries, Has.Length.EqualTo(1));
				Assert.That(revoke.Entries[0].SerialNumber, Is.EqualTo("008000000000000000"));
				Assert.That(revoke.Entries[0].RevocationDate, Is.EqualTo(after.AddSeconds(10)));
				Assert.That(revoke.Entries[0].Reason, Is.EqualTo(CertificateRevocationReason.Superseded));
			});

			Assert.Multiple(() =>
			{
				var cert = actual.Certificates.ElementAt(1);

				Assert.That(cert.SerialNumber, Is.EqualTo("0066"));
				Assert.That(cert.Issuer, Is.EqualTo("CN=No.101 Test Intermediate CA Certificate"));
				Assert.That(cert.Subject, Is.EqualTo("CN=No.102 Test Intermediate CA Certificate"));
				Assert.That(cert.NotBefore, Is.EqualTo(before));
				Assert.That(cert.NotAfter, Is.EqualTo(after));
				Assert.That(cert.IsCa, Is.True);
				Assert.That(cert.HcRole, Is.Null);
				Assert.That(cert.ExtendedKeyUsage, Is.Null);

				Assert.That(cert.Revocations, Has.Length.EqualTo(1));
				var revoke = cert.Revocations.First();
				Assert.That(revoke.DataType, Is.EqualTo(CertificateRevocationDataType.CRL));
				Assert.That(revoke.Issuer, Is.EqualTo("CN=No.101 Test Intermediate CA Certificate"));
				Assert.That(revoke.ThisUpdate, Is.EqualTo(utcDateTime));
				Assert.That(revoke.NextUpdate, Is.EqualTo(utcDateTime.AddSeconds(100)));
				Assert.That(revoke.Entries, Has.Length.EqualTo(0));
			});

			Assert.Multiple(() =>
			{
				var cert = actual.Certificates.ElementAt(2);

				Assert.That(cert.SerialNumber, Is.EqualTo("0065"));
				Assert.That(cert.Issuer, Is.EqualTo("CN=No.100 Test Intermediate CA Certificate"));
				Assert.That(cert.Subject, Is.EqualTo("CN=No.101 Test Intermediate CA Certificate"));
				Assert.That(cert.NotBefore, Is.EqualTo(before));
				Assert.That(cert.NotAfter, Is.EqualTo(after));
				Assert.That(cert.IsCa, Is.True);
				Assert.That(cert.HcRole, Is.Null);
				Assert.That(cert.ExtendedKeyUsage, Is.Null);

				Assert.That(cert.Revocations, Has.Length.EqualTo(1));
				var revoke = cert.Revocations.First();
				Assert.That(revoke.DataType, Is.EqualTo(CertificateRevocationDataType.OCSP));
				Assert.That(revoke.Issuer, Is.EqualTo("CN=No.100 Test Intermediate CA Certificate"));
				Assert.That(revoke.ThisUpdate, Is.EqualTo(utcDateTime));
				Assert.That(revoke.NextUpdate, Is.EqualTo(utcDateTime.AddSeconds(100)));
				Assert.That(revoke.Entries, Has.Length.EqualTo(1));
				Assert.That(revoke.Entries[0].SerialNumber, Is.EqualTo("0065"));
				Assert.That(revoke.Entries[0].RevocationDate, Is.EqualTo(utcDateTime));
				Assert.That(revoke.Entries[0].Reason, Is.EqualTo(CertificateRevocationReason.AACompromise));
			});

			Assert.Multiple(() =>
			{
				var cert = actual.Certificates.ElementAt(3);

				Assert.That(cert.SerialNumber, Is.EqualTo("0064"));
				Assert.That(cert.Issuer, Is.EqualTo("CN=Test Root CA Certificate"));
				Assert.That(cert.Subject, Is.EqualTo("CN=No.100 Test Intermediate CA Certificate"));
				Assert.That(cert.NotBefore, Is.EqualTo(before));
				Assert.That(cert.NotAfter, Is.EqualTo(after));
				Assert.That(cert.IsCa, Is.True);
				Assert.That(cert.HcRole, Is.Null);
				Assert.That(cert.ExtendedKeyUsage, Is.Null);

				Assert.That(cert.Revocations, Has.Length.EqualTo(1));
				var revoke = cert.Revocations.First();
				Assert.That(revoke.DataType, Is.EqualTo(CertificateRevocationDataType.CRL));
				Assert.That(revoke.Issuer, Is.EqualTo("CN=Test Root CA Certificate"));
				Assert.That(revoke.ThisUpdate, Is.EqualTo(utcDateTime));
				Assert.That(revoke.NextUpdate, Is.EqualTo(utcDateTime.AddSeconds(100)));
				Assert.That(revoke.Entries, Has.Length.EqualTo(0));
			});

			Assert.Multiple(() =>
			{
				var cert = actual.Certificates.ElementAt(4);

				Assert.That(cert.SerialNumber, Is.EqualTo("01"));
				Assert.That(cert.Issuer, Is.EqualTo("CN=Test Root CA Certificate"));
				Assert.That(cert.Subject, Is.EqualTo("CN=Test Root CA Certificate"));
				Assert.That(cert.NotBefore, Is.EqualTo(before));
				Assert.That(cert.NotAfter, Is.EqualTo(after));
				Assert.That(cert.IsCa, Is.True);
				Assert.That(cert.HcRole, Is.Null);
				Assert.That(cert.ExtendedKeyUsage, Is.Null);

				Assert.That(cert.Revocations, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = actual.ResultItems.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("item-reference-1"));
				Assert.That(item.Message, Is.EqualTo("message-reference-1"));
			});

			Assert.Multiple(() =>
			{
				var item = actual.ResultItems.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(item.ItemName, Is.EqualTo("item-reference-2"));
				Assert.That(item.Message, Is.EqualTo("message-reference-2"));
			});

			Assert.Multiple(() =>
			{
				var item = actual.ResultItems.ElementAt(2);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.ItemName, Is.EqualTo("item-reference-3"));
				Assert.That(item.Message, Is.EqualTo("message-reference-3"));
			});

			return;
		}


		[Test]
		public void Generate_WhenEmpty()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var config = new ReportConfig();

			var pathValidationData = new CertificatePathValidationData(null, null, null, null);

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.SetupGet(x => x.Certificate).Returns((CertificateData)null);
			validationData.SetupGet(x => x.PathValidationData).Returns(pathValidationData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.DispPrescription);
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.A);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var result = new VerificationResult(VerificationStatus.VALID, null);

			var target = new SigningCertificateReporter(config);

			//Act.
			var actual = target.Generate(signature.Object, utcDateTime, result);

			//Assert
			TestContext.WriteLine(TestData.ToJson(actual, DateTimeZoneHandling.RoundtripKind));

			signature.Verify();
			validationData.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(actual.Id, Is.Null);
				Assert.That(actual.CertificateBaseTime, Is.EqualTo(utcDateTime));
				Assert.That(actual.Certificates, Is.Empty);
				Assert.That(actual.ResultItems, Is.Null);
			});

			return;
		}
	}
}
