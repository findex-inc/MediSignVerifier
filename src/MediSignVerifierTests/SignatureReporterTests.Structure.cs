using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Reports.Reporters;
using MediSignVerifier.Tests.Properties;
using SignatureVerifier.Verifiers.StructureVerifiers;

namespace SignatureVerifier
{
	internal partial class SignatureReporterTests
	{
		[Test(Description = "XMLスキーマ検証レポート(処方) - VALID")]
		public void Generate_Prescription_Schema()
		{
			var doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription_004_01.xml");

			var schemaVerifier = new XmlSchemaVerifier(verifyConf);
			var schemaResult = schemaVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(schemaResult.Items.Count, Is.EqualTo(0));
			});

			//None
			var report = reporter.Generate(SignatureSourceType.None, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			var schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Prescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Prescription, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Dispensing(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//DispPrescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});
		}

		[Test(Description = "XMLスキーマ検証レポート(調剤) - VALID")]
		public void Generate_Dispensing_Schema()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var schemaVerifier = new XmlSchemaVerifier(verifyConf);
			var schemaResult = schemaVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(schemaResult.Items.Count, Is.EqualTo(0));
			});

			//None
			var report = reporter.Generate(SignatureSourceType.None, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			var schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Prescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Prescription, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Dispensing(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//DispPrescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});
		}

		[Test(Description = "XMLスキーマ検証レポート(処方) - INVALID")]
		public void Generate_Prescription_SchemaError()
		{
			var doc = TestData.CreateXmlDocument(Properties.Resources_XmlSchema.XmlSchemaPrescriptionError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "XmlSchemaPrescriptionError.xml");

			var schemaVerifier = new XmlSchemaVerifier(verifyConf);
			var schemaResult = schemaVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(schemaResult.Items.Count, Is.EqualTo(1));
			});

			//None
			var report = reporter.Generate(SignatureSourceType.None, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			var schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(1));
			});

			//Prescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Prescription, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Dispensing(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//DispPrescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});
		}

		[Test(Description = "XMLスキーマ検証レポート(調剤) - INVALID")]
		public void Generate_Dispensing_SchemaError()
		{
			var doc = TestData.CreateXmlDocument(Properties.Resources_XmlSchema.XmlSchemaDispensingError);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "XmlSchemaDispensingError.xml");

			var schemaVerifier = new XmlSchemaVerifier(verifyConf);
			var schemaResult = schemaVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(schemaResult.Items.Count, Is.EqualTo(2));
			});

			//None
			var report = reporter.Generate(SignatureSourceType.None, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			var schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(2));
			});

			//Prescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Prescription, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//Dispensing(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});

			//DispPrescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, schemaResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			schemaItems = report.ResultItems.Where(m => m.ItemName == "XMLスキーマ");
			Assert.Multiple(() =>
			{
				Assert.That(schemaItems.Count(), Is.EqualTo(0));
			});
		}

		[Test(Description = "XAdES構造検証レポート(処方) - VALID")]
		public void Generate_Prescription_XAdES()
		{
			var doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription_004_01.xml");

			var xadesVerifier = new XAdESStructureVerifier(verifyConf);
			var xadesResult = xadesVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(xadesResult.Items.Count, Is.EqualTo(2));
			});

			//None(無いはず)
			var report = reporter.Generate(SignatureSourceType.None, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.Count, Is.EqualTo(0));
			});

			//Prescription
			report = reporter.Generate(SignatureSourceType.Prescription, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			var xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(1));
				Assert.That(xadesItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});

			var signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(1));
				Assert.That(signPropItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Dispensing(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(0));
			});

			signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(0));
			});

			//DispPrescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Dispensing, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(0));
			});

			signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(0));
			});
		}

		[Test(Description = "XAdES構造検証レポート(調剤) - VALID")]
		public void Generate_Dispensing_XAdES()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var verifyConf = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing_005_03.xml");

			var xadesVerifier = new XAdESStructureVerifier(verifyConf);
			var xadesResult = xadesVerifier.Verify(data);

			var reportConf = new ReportConfig();
			var reporter = new StructureReporter(reportConf);

			//検証結果件数
			Assert.Multiple(() =>
			{
				Assert.That(xadesResult.Items.Count, Is.EqualTo(4));
			});

			//None(無いはず)
			var report = reporter.Generate(SignatureSourceType.None, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:None"));
			TestContext.WriteLine(TestData.ToJson(report));

			Assert.Multiple(() =>
			{
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.Count, Is.EqualTo(0));
			});

			//Prescription(無いはず)
			report = reporter.Generate(SignatureSourceType.Prescription, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Prescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			var xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(0));
			});

			var signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(0));
			});

			//Dispensing
			report = reporter.Generate(SignatureSourceType.Dispensing, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:Dispensing"));
			TestContext.WriteLine(TestData.ToJson(report));

			xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(1));
				Assert.That(xadesItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});

			signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(1));
				Assert.That(xadesItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//DispPrescription
			report = reporter.Generate(SignatureSourceType.Dispensing, xadesResult);
			TestContext.WriteLine(TestData.ToJson("SourceType:DispPrescription"));
			TestContext.WriteLine(TestData.ToJson(report));

			xadesItems = report.ResultItems.Where(m => m.ItemName == "XAdES必須要素");
			Assert.Multiple(() =>
			{
				Assert.That(xadesItems.Count(), Is.EqualTo(1));
				Assert.That(xadesItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});

			signPropItems = report.ResultItems.Where(m => m.ItemName == "SignedProperties要素");
			Assert.Multiple(() =>
			{
				Assert.That(signPropItems.Count(), Is.EqualTo(1));
				Assert.That(xadesItems.First().Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}
	}
}
