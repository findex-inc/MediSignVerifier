using System;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Reports.Reporters;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier
{
	partial class SignatureReporterTests
	{
		[Test(Description = "署名者証明書レポート(処方) = VALID")]
		public void Generate_Prescription_SigningCertificate()
		{
			var validationTime = DateTime.MaxValue;

			var data = new SignedDocumentXml(
				TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Prescription_004_01),
				"Prescription_004_01.xml");

			Assert.Multiple(() =>
			{
				Assert.That(data.DocumentType, Is.EqualTo(DocumentType.Prescription));
				Assert.That(data.Signatures.Count(), Is.EqualTo(1));
			});

			var verifyConf = new VerificationConfig();
			var verifier = new SigningCertificateVerifier(verifyConf);

			var verifyResult = verifier.Verify(data, validationTime);

			Assert.Multiple(() =>
			{
				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(5));
			});

			var reportConf = new ReportConfig();
			var reporter = new SigningCertificateReporter(reportConf);

			var signature = data.Signatures.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(signature.ESLevel, Is.EqualTo(ESLevel.XL));
			});

			var report = reporter.Generate(signature, validationTime, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report, DateTimeZoneHandling.RoundtripKind));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id6a875f31-KeyInfo"));
				Assert.That(report.CertificateBaseTime, Is.EqualTo(signature.SignatureTimeStampGenTime));
				Assert.That(report.Certificates, Has.Length.EqualTo(3));
				Assert.That(report.ResultItems, Has.Length.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(0);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の指定確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(1);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の実体確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(2);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の一致確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(3);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("HPKI固有項目の確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(4);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
					Assert.That(item.Message, Is.Null);
				});
			});
		}

		[Test(Description = "署名タイムスタンプ検証レポート(調剤) = VALID")]
		public void Generate_Dispensing_SigningCertificate()
		{
			var validationTime = DateTime.MaxValue;

			var data = new SignedDocumentXml(
				TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Dispensing_005_03),
				"Dispensing_005_03.xml");

			Assert.Multiple(() =>
			{
				Assert.That(data.DocumentType, Is.EqualTo(DocumentType.Dispensing));
				Assert.That(data.Signatures.Count(), Is.EqualTo(2));
			});

			var verifyConf = new VerificationConfig();
			var verifier = new SigningCertificateVerifier(verifyConf);

			var verifyResult = verifier.Verify(data, validationTime);

			Assert.Multiple(() =>
			{
				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(10));
			});

			var reportConf = new ReportConfig();
			var reporter = new SigningCertificateReporter(reportConf);

			//署名１(Dispensing)
			var dispensingSignature = data.Signatures.First(m => m.SourceType == SignatureSourceType.Dispensing);

			Assert.Multiple(() =>
			{
				Assert.That(dispensingSignature.ESLevel, Is.EqualTo(ESLevel.A));
			});

			var dispensingReport = reporter.Generate(dispensingSignature, validationTime, verifyResult);

			TestContext.WriteLine($"SourceType:{dispensingSignature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(dispensingReport, DateTimeZoneHandling.RoundtripKind));

			Assert.Multiple(() =>
			{
				Assert.That(dispensingReport.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(dispensingReport.Id, Is.EqualTo("id771d57b8-KeyInfo"));
				Assert.That(dispensingReport.CertificateBaseTime, Is.EqualTo(dispensingSignature.SignatureTimeStampGenTime));
				Assert.That(dispensingReport.Certificates, Has.Length.EqualTo(3));
				Assert.That(dispensingReport.ResultItems, Has.Length.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				var report = dispensingReport;

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(0);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の指定確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(1);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の実体確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(2);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の一致確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(3);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("HPKI固有項目の確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(4);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
					Assert.That(item.Message, Is.Null);
				});
			});

			//署名２(DispPrescription)
			var prescriptionSignature = data.Signatures.First(m => m.SourceType == SignatureSourceType.DispPrescription);

			Assert.Multiple(() =>
			{
				Assert.That(prescriptionSignature.ESLevel, Is.EqualTo(ESLevel.XL));
			});

			var prescriptionReport = reporter.Generate(prescriptionSignature, validationTime, verifyResult);

			TestContext.WriteLine($"SourceType:{prescriptionSignature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(prescriptionReport, DateTimeZoneHandling.RoundtripKind));

			Assert.Multiple(() =>
			{
				Assert.That(dispensingReport.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(dispensingReport.Id, Is.EqualTo("id771d57b8-KeyInfo"));
				Assert.That(dispensingReport.CertificateBaseTime, Is.EqualTo(dispensingSignature.SignatureTimeStampGenTime));
				Assert.That(dispensingReport.Certificates, Has.Length.EqualTo(3));
				Assert.That(dispensingReport.ResultItems, Has.Length.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				var report = dispensingReport;

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(0);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の指定確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(1);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の実体確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(2);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書の一致確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(3);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("HPKI固有項目の確認"));
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = report.ResultItems.ElementAt(4);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
					Assert.That(item.Message, Is.Null);
				});
			});
		}

	}
}
