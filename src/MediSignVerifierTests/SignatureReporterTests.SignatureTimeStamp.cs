using System;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Reports.Reporters;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier
{
	internal partial class SignatureReporterTests
	{
		[Test(Description = "署名タイムスタンプ検証レポート(処方)")]
		public void Generate_Prescription_SignatureTimeStamp()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription_004_01.xml");

			var verifier = new SignatureTimeStampVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(2));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureTimeStampReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("idbc9e4f38"));
				Assert.That(report.C14nAlgorithm, Is.Null);
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("SHA-512withRSA"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

				//Certificates
				Assert.That(report.TsaCertificateInfo.Certificates.Count, Is.EqualTo(2));


				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "署名タイムスタンプ検証レポート(調剤)")]
		public void Generate_Dispensing_SignatureTimeStamp()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var verifier = new SignatureTimeStampVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(2));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(4));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureTimeStampReporter(reportConf);

			//署名１(Dispensing)
			var signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.Dispensing);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id96e0a365"));
				Assert.That(report.C14nAlgorithm, Is.Null);
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("SHA-512withRSA"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

				//Certificates
				Assert.That(report.TsaCertificateInfo.Certificates.Count, Is.EqualTo(2));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//署名２(DispPrescription)
			signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.DispPrescription);
			report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("idbc9e4f38"));
				Assert.That(report.C14nAlgorithm, Is.Null);
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("SHA-512withRSA"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

				//Certificates
				Assert.That(report.TsaCertificateInfo.Certificates.Count, Is.EqualTo(2));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

	}
}
