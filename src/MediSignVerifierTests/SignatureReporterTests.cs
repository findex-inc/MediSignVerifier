using System;
using System.Linq;
using NUnit.Framework;
using MediSignVerifier.Tests.Properties;
using static SignatureVerifier.SignatureVerificationReportExtensions;

namespace SignatureVerifier
{
	internal partial class SignatureReporterTests
	{
		private static readonly SignatureVerificationReportSettings _jsonSettings = new SignatureVerificationReportSettings
		{
			DateTimeZoneHandling = DateTimeZoneHandling.Local,
			FormatRequired = true,
		};

		//TODO 署名検証に成功するXMLが欲しい。。
		//TODO 署名検証がINDETERMINATEになるXMLが欲しい。

		[Test(Description = "(処方)検証結果レポート作成")]
		public void Generate_Prescription()
		{
			var xml = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "Prescription_004_01.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				//ATS証明書のエラーがあるためINVALID
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Message, Is.EqualTo("署名検証に成功しました。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Prescription));
				Assert.That(report.Signatures.Count, Is.EqualTo(1));
			});

			var signature = report.Signatures[0];

			Assert.Multiple(() =>
			{
				Assert.That(signature.SourceType, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(signature.ESLevel, Is.EqualTo(ESLevel.XL));

				Assert.That(signature.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//検証基準時刻（検証時刻）
			Assert.That(signature.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));
		}

		[Test(Description = "(処方)検証結果レポート作成 - スキーマエラー")]
		public void Generate_PrescriptionError()
		{
			var xml = TestData.CreateXmlDocument(Properties.Resources_XmlSchema.XmlSchemaPrescriptionError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "XmlSchemaPrescriptionError.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("署名構造の検証に失敗しました。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Prescription));
				Assert.That(report.Signatures, Is.Null);
			});
		}

		[Test(Description = "(調剤)検証結果レポート作成")]
		public void Generate_Dispensing()
		{
			var xml = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "Dispensing_005_03.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(report.Message, Is.EqualTo("署名検証に必要な情報が不足しています。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Dispensing));
				Assert.That(report.Signatures.Count, Is.EqualTo(2));
			});

			//DispPrescription
			var signature1 = report.Signatures[0];

			Assert.Multiple(() =>
			{
				Assert.That(signature1.SourceType, Is.EqualTo(SignatureSourceType.DispPrescription));
				Assert.That(signature1.ESLevel, Is.EqualTo(ESLevel.XL));

				Assert.That(signature1.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));

			});

			//Dispensing
			var signature2 = report.Signatures[1];

			Assert.Multiple(() =>
			{
				Assert.That(signature2.SourceType, Is.EqualTo(SignatureSourceType.Dispensing));
				Assert.That(signature2.ESLevel, Is.EqualTo(ESLevel.A));

				Assert.That(signature2.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.ArchiveTimeStamps.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
			});

			//検証基準時刻
			Assert.Multiple(() =>
			{
				//署名タイムスタンプ
				//調剤内処方：ES-Aの署名タイムスタンプ時刻
				Assert.That(signature1.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature2.SignatureTimeStamp.TimeStampGenTime));

				//調剤：最初のATS時刻
				Assert.That(signature2.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature2.ArchiveTimeStamps.Items.ElementAt(0).TimeStampGenTime));

				//アーカイブタイムスタンプ：検証時刻
				Assert.That(signature2.ArchiveTimeStamps.Items.ElementAt(0).TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));
			});
		}

		[Test(Description = "(調剤＆その他)検証結果レポート作成")]
		public void Generate_DispensingAndUnknown()
		{
			var xml = TestData.CreateXmlDocument(Resources.Dispensing_ScanMixed);

			var verifyConf = new VerificationConfig() { HPKIValidationEnabled = false };
			var data = new SignedDocumentXml(xml, "Dispensing_ScanMixed.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("規定以外の署名が存在します。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Dispensing));
				Assert.That(report.Signatures.Count, Is.EqualTo(3));
			});

			//DispPrescription
			var signature1 = report.Signatures[0];

			Assert.Multiple(() =>
			{
				Assert.That(signature1.SourceType, Is.EqualTo(SignatureSourceType.DispPrescription));
				Assert.That(signature1.ESLevel, Is.EqualTo(ESLevel.XL));

				Assert.That(signature1.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature1.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Dispensing
			var signature2 = report.Signatures[1];

			Assert.Multiple(() =>
			{
				Assert.That(signature2.SourceType, Is.EqualTo(SignatureSourceType.Dispensing));
				Assert.That(signature2.ESLevel, Is.EqualTo(ESLevel.A));

				Assert.That(signature2.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature2.ArchiveTimeStamps.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
			});

			//Other(無理やりXMLに追加したので軒並みエラー)
			var signature3 = report.Signatures[2];

			Assert.Multiple(() =>
			{
				Assert.That(signature3.SourceType, Is.EqualTo(SignatureSourceType.Unknown));
				Assert.That(signature3.ESLevel, Is.EqualTo(ESLevel.A));

				Assert.That(signature3.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature3.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature3.References.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(signature3.SignatureValue.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(signature3.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(signature3.ArchiveTimeStamps.Status, Is.EqualTo(VerificationStatus.INVALID));
			});

			//検証基準時刻
			Assert.Multiple(() =>
			{
				//署名タイムスタンプ
				//調剤内処方：調剤の署名TS時刻
				Assert.That(signature1.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature2.SignatureTimeStamp.TimeStampGenTime));

				//調剤：最初のATS時刻
				Assert.That(signature2.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature2.ArchiveTimeStamps.Items.ElementAt(0).TimeStampGenTime));

				//その他：最初のATS時刻
				Assert.That(signature3.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature3.ArchiveTimeStamps.Items.ElementAt(0).TimeStampGenTime));

				//アーカイブタイムスタンプ
				//調剤：検証時刻
				Assert.That(signature2.ArchiveTimeStamps.Items.ElementAt(0).TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));

				//その他：検証時刻
				Assert.That(signature3.ArchiveTimeStamps.Items.ElementAt(0).TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));

			});
		}

		[Test(Description = "(調剤)検証結果レポート作成 - スキーマエラー")]
		public void Generate_DispensingError()
		{
			var xml = TestData.CreateXmlDocument(Properties.Resources_XmlSchema.XmlSchemaDispensingError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "XmlSchemaDispensingError.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("署名構造の検証に失敗しました。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Dispensing));
				Assert.That(report.Signatures, Is.Null);
			});
		}

		[Test(Description = "(その他)検証結果レポート作成：複数アーカイブタイムスタンプ")]
		public void Generate_UnknownMultiATS()
		{
			var xml = TestData.CreateXmlDocument(Resources.Scan_20170511154438586_ESXLA);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "Scan_20170511154438586_ESXLA.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				//規定の署名が存在しないためINVALID
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("規定の署名が存在しませんでした。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Unknown));
				Assert.That(report.Signatures.Count, Is.EqualTo(1));
			});

			var signature = report.Signatures[0];

			Assert.Multiple(() =>
			{
				Assert.That(signature.SourceType, Is.EqualTo(SignatureSourceType.Unknown));
				Assert.That(signature.ESLevel, Is.EqualTo(ESLevel.A));

				Assert.That(signature.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.ArchiveTimeStamps.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
			});

			//アーカイブタイムスタンプ
			Assert.That(signature.ArchiveTimeStamps.Items.Count, Is.EqualTo(4));

			//検証基準時刻
			Assert.Multiple(() =>
			{
				//署名タイムスタンプ：最初のATS時刻
				Assert.That(signature.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature.ArchiveTimeStamps.Items.ElementAt(0).TimeStampGenTime));

				//アーカイブタイムスタンプ：次世代のATS時刻
				Assert.That(signature.ArchiveTimeStamps.Items.ElementAt(1).TsaCertificateBaseTime, Is.EqualTo(signature.ArchiveTimeStamps.Items.ElementAt(2).TimeStampGenTime));
				Assert.That(signature.ArchiveTimeStamps.Items.ElementAt(2).TsaCertificateBaseTime, Is.EqualTo(signature.ArchiveTimeStamps.Items.ElementAt(3).TimeStampGenTime));

				//アーカイブタイムスタンプ(最終)：検証時刻
				Assert.That(signature.ArchiveTimeStamps.Items.ElementAt(3).TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));
			});
		}

		[Test(Description = "(その他)検証結果レポート作成：単一アーカイブタイムスタンプ")]
		public void Generate_UnknownSingleATS()
		{
			var xml = TestData.CreateXmlDocument(Resources.Scan_20190207203737336_ESXLA);

			var verifyConf = new VerificationConfig() { HPKIValidationEnabled = false };
			var data = new SignedDocumentXml(xml, "Scan_20190207203737336_ESXLA.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				//規定の署名が存在しないためINVALID
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("規定の署名が存在しませんでした。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Unknown));
				Assert.That(report.Signatures.Count, Is.EqualTo(1));
			});

			var signature = report.Signatures[0];

			Assert.Multiple(() =>
			{
				Assert.That(signature.SourceType, Is.EqualTo(SignatureSourceType.Unknown));
				Assert.That(signature.ESLevel, Is.EqualTo(ESLevel.A));

				Assert.That(signature.Structure.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SigningCertificate.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.References.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureValue.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.SignatureTimeStamp.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(signature.ArchiveTimeStamps.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
			});

			//検証基準時刻
			Assert.Multiple(() =>
			{
				//署名タイムスタンプ：最初のATS時刻
				Assert.That(signature.SignatureTimeStamp.TsaCertificateBaseTime, Is.EqualTo(signature.ArchiveTimeStamps.Items.ElementAt(0).TimeStampGenTime));
				//アーカイブタイムスタンプ：検証時刻
				Assert.That(signature.ArchiveTimeStamps.Items.ElementAt(0).TsaCertificateBaseTime, Is.EqualTo(verificationTime.ToUniversalTime()));
			});
		}

		[Test(Description = "(署名なし)検証結果レポート作成")]
		public void Generate_NoSignatureError()
		{
			var xml = TestData.CreateXmlDocument(Resources.NotFoundSignature);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "NotFoundSignature.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("検証できる署名が存在しませんでした。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Unknown));
				Assert.That(report.Signatures, Is.Null);
			});
		}

		[Test(Description = "(その他)検証結果レポート作成：欧州実装XML")]
		public void Generate_UnknownEuropeSignature()
		{
			var xml = TestData.CreateXmlDocument(Resources.Europe);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(xml, "Europe.xml");

			var verificationTime = DateTime.Now;
			var verifier = new SignatureVerifier(verifyConf);
			var result = verifier.Verify(data, verificationTime);

			//検証実行後、レポート作成
			var reportConf = new ReportConfig();
			var reporter = new SignatureReporter(reportConf);
			var report = reporter.Generate(data, verificationTime, result);

			var json = report.ToJson(_jsonSettings);

			TestContext.WriteLine(json);

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Message, Is.EqualTo("規定の署名が存在しませんでした。"));
				Assert.That(report.DocumentType, Is.EqualTo(DocumentType.Unknown));
				Assert.That(report.Signatures.Count, Is.EqualTo(1));

				//Reference(XPath)
				var sigReport = report.Signatures.ElementAt(0);
				Assert.That(sigReport.References.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}
	}
}
