using System;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Properties;
using SignatureVerifier.Reports;
using SignatureVerifier.Reports.Reporters;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier
{
	internal partial class SignatureReporterTests
	{
		[Test(Description = "参照データ検証レポート(処方) = VALID")]
		public void Generate_Prescription_Reference()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription_004_01.xml");

			var verifier = new ReferenceVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(6));
			});

			var reportConf = new ReportConfig();
			var reporter = new ReferenceReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			var report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id29b12603-Reference1-Detached"));
				Assert.That(report.Uri, Is.EqualTo("#PrescriptionDocument"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("default(C14N)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id041cb21b-Reference2-KeyInfo"));
				Assert.That(report.Uri, Is.EqualTo("#id6a875f31-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id8b2ae0fe-Reference3-SignedProperties"));
				Assert.That(report.Uri, Is.EqualTo("#idc51bfd03-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

		}

		[Test(Description = "参照データ検証レポート(調剤) = VALID")]
		public void Generate_Dispensing_Reference()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var verifier = new ReferenceVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(2));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(12));
			});

			var reportConf = new ReportConfig();
			var reporter = new ReferenceReporter(reportConf);

			//署名１(Dispensing)
			var signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.Dispensing);
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			var report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("iddb33cd61-Reference1-Detached"));
				Assert.That(report.Uri, Is.EqualTo("#Dispensing"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("default(C14N)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("idfbb8185d-Reference2-KeyInfo"));
				Assert.That(report.Uri, Is.EqualTo("#id771d57b8-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id5813f70a-Reference3-SignedProperties"));
				Assert.That(report.Uri, Is.EqualTo("#id111a1e5c-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//署名２(DispPrescription)
			signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.DispPrescription);
			reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id29b12603-Reference1-Detached"));
				Assert.That(report.Uri, Is.EqualTo("#PrescriptionDocument"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("default(C14N)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id041cb21b-Reference2-KeyInfo"));
				Assert.That(report.Uri, Is.EqualTo("#id6a875f31-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("id8b2ae0fe-Reference3-SignedProperties"));
				Assert.That(report.Uri, Is.EqualTo("#idc51bfd03-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertReferenceResultItems(report.ResultItems);
			});

		}

		private static void AssertReferenceResultItems(VerificationResultReportItem[] reportItems)
		{
			Assert.That(reportItems.Count, Is.EqualTo(2));

			//Reference要素
			var items = reportItems.Where(m => m.ItemName == "Reference要素");

			Assert.Multiple(() =>
			{
				Assert.That(items.Count, Is.EqualTo(1));

				var item = items.ElementAt(0);
				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.Message, Is.Null);
			});

			//DigestMethod要素
			items = reportItems.Where(m => m.ItemName == "DigestMethod要素");

			Assert.Multiple(() =>
			{
				Assert.That(items.Count, Is.EqualTo(1));

				var item = items.ElementAt(0);
				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.Message, Is.Null);
			});
		}

		[Test(Description = "参照データ検証レポート(処方) = INVALID")]
		public void Generate_Prescription_ReferenceError()
		{
			var doc = TestData.CreateXmlDocument(Resources_Reference.PrescriptionReferenceError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "PrescriptionReferenceError.xml");

			var verifier = new ReferenceVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(6));
			});

			var reportConf = new ReportConfig();
			var reporter = new ReferenceReporter(reportConf);

			//署名１
			var signature = data.Signatures.ElementAt(0);
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			var report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("ハッシュ値の計算に失敗しました。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Message, Is.EqualTo("サポートされていないダイジェストアルゴリズムが指定されています。"));
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("DigestValueのデコードに失敗しました。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "参照データ検証レポート(調剤) = VALID")]
		public void Generate_Dispensing_ReferenceError()
		{
			var doc = TestData.CreateXmlDocument(Resources_Reference.DispensingReferenceError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "DispensingReferenceError.xml");

			var verifier = new ReferenceVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(2));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(12));
			});

			var reportConf = new ReportConfig();
			var reporter = new ReferenceReporter(reportConf);

			//署名１(Dispensing)
			var signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.Dispensing);
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			var report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//署名２（DispPrescription）
			signature = data.Signatures.First(m => m.SourceType == SignatureSourceType.DispPrescription);
			reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(3));
			});

			//Reference1
			report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
			});

			//Reference2
			report = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Reference3
			report = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
			});

		}

	}
}
