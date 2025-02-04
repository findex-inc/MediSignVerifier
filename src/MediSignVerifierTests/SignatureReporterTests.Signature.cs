using System;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Properties;
using SignatureVerifier.Reports.Reporters;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier
{
	internal partial class SignatureReporterTests
	{
		[Test(Description = "署名データ検証レポート(処方) = VALID")]
		public void Generate_Prescription_Signature()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription_004_01.xml");

			var verifier = new SignatureValueVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(2));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureValueReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id00fac23c-SignatureValue"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "署名データ検証レポート(調剤) = VALID")]
		public void Generate_Dispensing_Signature()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var verifier = new SignatureValueVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(2));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(4));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureValueReporter(reportConf);

			//署名１(Dispensing)
			var signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.Dispensing);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id218934fe-SignatureValue"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//署名２(DispPrescription)
			signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.DispPrescription);
			report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id00fac23c-SignatureValue"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "署名データ検証レポート = INVALID(証明書改ざんエラー)")]
		public void Generate_Signature_KeyInfoError()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureValue.KeyInfoError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "KeyInfoError.xml");

			var verifier = new SignatureValueVerifier(config);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(2));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureValueReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("id00fac23c-SignatureValue"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Message, Is.EqualTo("署名値の復号に失敗しました。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "署名データ検証レポート = INVALID(正規化エラー)")]
		public void Generate_Signature_SignedInfoError()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureValue.SignedInfoError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "SignedInfoError.xml");

			var verifier = new SignatureValueVerifier(config);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(2));
			});

			var reportConf = new ReportConfig();
			var reporter = new SignatureValueReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var report = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("id00fac23c-SignatureValue"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo(""));
				Assert.That(report.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Message, Is.EqualTo("正規化アルゴリズムが不正です。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

	}
}
