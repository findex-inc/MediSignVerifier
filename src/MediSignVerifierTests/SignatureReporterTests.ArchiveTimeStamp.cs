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
		[Test(Description = "アーカイブタイムスタンプ検証レポート(調剤)")]
		public void Generate_Dispensing_ArchiveTimeStamp()
		{
			var doc = TestData.CreateXmlDocument(MediSignVerifier.Tests.Properties.Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var verifier = new ArchiveTimeStampVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			var dispSignature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(data.Signatures.Count, Is.EqualTo(2));

				Assert.That(dispSignature, Is.Not.Null);
				Assert.That(dispSignature.ArchiveTimeStampValidationData, Is.Not.Null);
				Assert.That(dispSignature.ArchiveTimeStampValidationData.Count, Is.EqualTo(1));

				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(2));
			});

			var reportConf = new ReportConfig();
			var reporter = new ArchiveTimeStampReporter(reportConf);

			//署名(調剤)
			var reportList = reporter.Generate(dispSignature, verifyResult);

			TestContext.WriteLine($"SourceType:{dispSignature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(1));
			});

			//ATS1
			var report = reportList.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(report.Id, Is.EqualTo("id97644ad5"));
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
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "TSA証明書")?.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "TSA証明書")?.Message, Does.StartWith("失効情報が発行されていません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));

			});
		}

		[Test(Description = "アーカイブタイムスタンプ検証レポート = INVALID(アーカイブタイムスタンプ要素無し)")]
		public void Generate_ArchiveTimeStamp_ExistsError()
		{
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundATS);

			var data = new SignedDocumentXml(doc, "NotFoundATS.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var verifyConf = new VerificationConfig();
			var verifier = new ArchiveTimeStampVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, DateTime.Now);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(1));
				Assert.That(verifyResult.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.ItemName == "")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.ItemName == "")?.Message, Is.EqualTo("アーカイブタイムスタンプが見つかりませんでした。"));
			});

			var reportConf = new ReportConfig();
			var reporter = new ArchiveTimeStampReporter(reportConf);

			//署名(調剤)
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(reportList.Message, Is.EqualTo("アーカイブタイムスタンプが見つかりませんでした。"));
				Assert.That(reportList.Items.Count, Is.EqualTo(0));
			});
		}

		[Test(Description = "アーカイブタイムスタンプ検証レポート = INVALID(タイムスタンプトークン改ざん)")]
		public void Generate_ArchiveTimeStamp_MultiTimeStampError()
		{
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.MultiATSBase64Error);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "MultiATSBase64Error.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Unknown);
			TestData.SetESLevel(ESLevel.A, signature);

			var validDate = DateTime.Now;
			var verifyConf = new VerificationConfig();
			var verifier = new ArchiveTimeStampVerifier(verifyConf);
			var verifyResult = verifier.Verify(data, validDate);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(verifyResult.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(verifyResult.Items.Count, Is.EqualTo(6));

				var ats1Results = verifyResult.Items.Where(m => m.Index == 1);
				Assert.That(ats1Results.Count, Is.EqualTo(1));


				var ats2Results = verifyResult.Items.Where(m => m.Index == 2);
				Assert.That(ats2Results.Count, Is.EqualTo(1));

				var ats3Results = verifyResult.Items.Where(m => m.Index == 3);
				Assert.That(ats3Results.Count, Is.EqualTo(2));

				var ats4Results = verifyResult.Items.Where(m => m.Index == 4);
				Assert.That(ats4Results.Count, Is.EqualTo(2));

			});

			var reportConf = new ReportConfig();
			var reporter = new ArchiveTimeStampReporter(reportConf);

			//署名(調剤)
			var reportList = reporter.Generate(signature, verifyResult);

			TestContext.WriteLine($"SourceType:{signature.SourceType}");
			TestContext.WriteLine(TestData.ToJson(reportList));

			Assert.Multiple(() =>
			{
				Assert.That(reportList.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(reportList.Message, Is.Null);
				Assert.That(reportList.Items.Count, Is.EqualTo(4));
			});

			//ATS1
			var report = reportList.Items.ElementAt(0);
			var nextGenReport = reportList.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Id-ATS-1"));
				Assert.That(report.TsaCertificateBaseTime, Is.Null);
				Assert.That(report.TimeStampGenTime, Is.Null);
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.Null);
				Assert.That(report.TsaSignatureInfo, Is.Null);
				Assert.That(report.TsaCertificateInfo, Is.Null);

				Assert.That(report.ResultItems.Count, Is.EqualTo(1));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "検証基準時刻")?.Message, Is.EqualTo("次世代アーカイブタイムスタンプ生成時刻の取得に失敗しました。"));
			});

			//ATS2
			report = reportList.Items.ElementAt(1);
			nextGenReport = reportList.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Id-ATS-2"));
				Assert.That(report.TsaCertificateBaseTime, Is.Null);
				Assert.That(report.TimeStampGenTime, Is.Null);
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.Null);
				Assert.That(report.TsaSignatureInfo, Is.Null);
				Assert.That(report.TsaCertificateInfo, Is.Null);

				Assert.That(report.ResultItems.Count, Is.EqualTo(1));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Message, Is.EqualTo("タイムスタンプのデコードに失敗しました。"));
			});

			//ATS3
			report = reportList.Items.ElementAt(2);
			nextGenReport = reportList.Items.ElementAt(3);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Id-ATS-3"));
				Assert.That(report.TsaCertificateBaseTime, Is.EqualTo(nextGenReport.TimeStampGenTime));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.CalculatedDispValue, Is.Not.Null);

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("RSA"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

				//Certificates
				Assert.That(report.TsaCertificateInfo.Certificates.Count, Is.EqualTo(2));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Message, Is.EqualTo("計算したハッシュ値とhashMessageの値が一致しません。"));
			});

			//ATS4
			report = reportList.Items.ElementAt(3);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(4));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Id-ATS-4"));
				Assert.That(report.TsaCertificateBaseTime, Is.EqualTo(validDate.ToUniversalTime()));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.CalculatedDispValue, Is.Not.Null);

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("RSA"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

				//Certificates
				Assert.That(report.TsaCertificateInfo.Certificates, Has.Length.EqualTo(1));
				Assert.That(report.TsaCertificateInfo.Certificates[0].SerialNumber, Is.EqualTo("0010"));
				Assert.That(report.TsaCertificateInfo.Certificates[0].Issuer, Is.EqualTo("C=JP,O=FINDEX,OU=FINDEX CA Root,CN=FINDEX CA Root"));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Message, Is.EqualTo("計算したハッシュ値とhashMessageの値が一致しません。"));
			});
		}
	}
}
