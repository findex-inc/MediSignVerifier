using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class ReferenceReporterTests
	{
		[Test(Description = "ValidationDataへの変換エラー")]
		public void Generate_Error()
		{
			//ResultItemが-1のものを出す（Signature単位で1回出るからReferencesは0件）
			//この場合は、ReferenceValidationDataはnull
			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "参照データ", -1, "", "", "不明なエラー"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ReferenceReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Message, Is.EqualTo("不明なエラー"));
				Assert.That(actual.Items.Count, Is.EqualTo(0));
			});
		}

		private static List<ReferenceValidationData> CreatePrescriptionValidationData()
		{
			return new List<ReferenceValidationData>
			{
				new ReferenceValidationData("Ref-001", 0, "#PrescriptionDocument",
					null, "http://www.w3.org/2001/04/xmlenc#sha256",
					"d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U=".ToBytes(), "d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U=".ToBytes()),
				new ReferenceValidationData("Ref-002", 1, "#id6a875f31-KeyInfo",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=".ToBytes(), "bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=".ToBytes()),
				new ReferenceValidationData("Ref-003", 2, "#idc51bfd03-SignedProperties",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=".ToBytes(), "8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=".ToBytes()),
			};
		}

		[Test(Description = "単一署名")]
		public void Generate_SingleSignature()
		{
			//TestData
			var references = CreatePrescriptionValidationData();

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.ReferenceValidationData).Returns(references);

			Assert.That(signature.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 1, "Reference要素", "Ref-001", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 1, "DigestMethod要素", "Ref-001", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 2, "Reference要素", "Ref-002", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 2, "DigestMethod要素", "Ref-002", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 3, "Reference要素", "Ref-003", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 3, "DigestMethod要素", "Ref-003", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ReferenceReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(処方)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Items.Count, Is.EqualTo(3));
			});

			AssertPrescription(actual);
		}

		private static void AssertPrescription(ReferenceReportList actual)
		{
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Items.Count, Is.EqualTo(3));
			});

			//Ref-001
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-001"));
				Assert.That(report.Uri, Is.EqualTo("#PrescriptionDocument"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("default(C14N)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-002
			report = actual.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-002"));
				Assert.That(report.Uri, Is.EqualTo("#id6a875f31-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-003
			report = actual.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-003"));
				Assert.That(report.Uri, Is.EqualTo("#idc51bfd03-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		private static List<ReferenceValidationData> CreateDispensingValidationData()
		{
			return new List<ReferenceValidationData>
			{
				new ReferenceValidationData("Ref-101", 0, "#Dispensing", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", "http://www.w3.org/2001/04/xmlenc#sha512",
					"qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU=".ToBytes(), "qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU=".ToBytes()),
				new ReferenceValidationData("Ref-102", 1, "#id771d57b8-KeyInfo",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg=".ToBytes(), "9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg=".ToBytes()),
				new ReferenceValidationData("Ref-103", 2, "#id111a1e5c-SignedProperties",
					"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", "http://www.w3.org/2001/04/xmldsig-more#sha384",
					"sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw=".ToBytes(), "sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw=".ToBytes()),
			};
		}

		[Test(Description = "複数署名")]
		public void Generate_MultiSignature()
		{
			//TestData(DispPrescription)
			var dispPrescriptionRefs = CreatePrescriptionValidationData();

			var dispPrescriptionSign = new Mock<ISignature>();
			dispPrescriptionSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.DispPrescription);
			dispPrescriptionSign.SetupGet(x => x.ReferenceValidationData).Returns(dispPrescriptionRefs);

			Assert.That(dispPrescriptionSign.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			//TestData(DIspensing)
			var dispensingRefs = CreateDispensingValidationData();

			var dispensingSign = new Mock<ISignature>();
			dispensingSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			dispensingSign.SetupGet(x => x.ReferenceValidationData).Returns(dispensingRefs);

			Assert.That(dispensingSign.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 1, "Reference要素", "Ref-101", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 1, "DigestMethod要素", "Ref-101", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 2, "Reference要素", "Ref-102", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 2, "DigestMethod要素", "Ref-102", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 3, "Reference要素", "Ref-103", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 3, "DigestMethod要素", "Ref-103", null),


				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 1, "Reference要素", "Ref-001", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 1, "DigestMethod要素", "Ref-001", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 2, "Reference要素", "Ref-002", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 2, "DigestMethod要素", "Ref-002", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 3, "Reference要素", "Ref-003", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 3, "DigestMethod要素", "Ref-003", null)

			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ReferenceReporter(config);

			//Act1(DispPrescription)
			var actual = target.Generate(dispPrescriptionSign.Object, result);

			TestContext.WriteLine("(調剤済み処方)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert1
			AssertPrescription(actual);

			//Act2(Dispensing)
			actual = target.Generate(dispensingSign.Object, result);

			TestContext.WriteLine("(調剤)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert2
			AssertDispensing(actual);

		}

		private static void AssertDispensing(ReferenceReportList actual)
		{
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Items.Count, Is.EqualTo(3));
			});

			//Ref-101
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-101"));
				Assert.That(report.Uri, Is.EqualTo("#Dispensing"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.DigestValue, Is.EqualTo("qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-102
			report = actual.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-102"));
				Assert.That(report.Uri, Is.EqualTo("#id771d57b8-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-103
			report = actual.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-103"));
				Assert.That(report.Uri, Is.EqualTo("#id111a1e5c-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N(with comments)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-384"));
				Assert.That(report.DigestValue, Is.EqualTo("sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		private static List<ReferenceValidationData> CreatePrescriptionErrorValidationData()
		{
			return new List<ReferenceValidationData>
			{
				new ReferenceValidationData("Ref-001", 0, "#PrescriptionDocument",
					null, "http://www.w3.org/2001/04/xmlenc#sha256",
					"d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U=".ToBytes(), "bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=".ToBytes()),
				new ReferenceValidationData("Ref-002", 1, "#id6a875f31-KeyInfo",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=".ToBytes(), "bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=".ToBytes()),
				new ReferenceValidationData("Ref-003", 2, "#idc51bfd03-SignedProperties",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=".ToBytes(), "8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=".ToBytes()),
			};
		}

		[Test(Description = "単一署名(検証エラーあり)")]
		public void Generate_SingleSignatureError()
		{
			//TestData
			var references = CreatePrescriptionErrorValidationData();

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.ReferenceValidationData).Returns(references);

			Assert.That(signature.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "参照データ", 1, "Reference要素", "Ref-001", "計算したハッシュ値とDigestValueの値が一致しません。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 1, "DigestMethod要素", "Ref-001", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 2, "Reference要素", "Ref-002", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 2, "DigestMethod要素", "Ref-002", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "参照データ", 3, "Reference要素", "Ref-003", null),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "参照データ", 3, "DigestMethod要素", "Ref-003", "サポートされていないダイジェストアルゴリズムが指定されています。"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ReferenceReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(処方)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			AssertPrescriptionError(actual);
		}

		private static void AssertPrescriptionError(ReferenceReportList actual)
		{
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count, Is.EqualTo(3));
			});

			//Ref-001
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Ref-001"));
				Assert.That(report.Uri, Is.EqualTo("#PrescriptionDocument"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("default(C14N)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U="));
				Assert.That(report.CalculatedDispValue, Is.EqualTo("bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ="));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-002
			report = actual.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-002"));
				Assert.That(report.Uri, Is.EqualTo("#id6a875f31-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-003
			report = actual.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Ref-003"));
				Assert.That(report.Uri, Is.EqualTo("#idc51bfd03-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Message, Is.EqualTo("サポートされていないダイジェストアルゴリズムが指定されています。"));
			});
		}

		private static List<ReferenceValidationData> CreateDispensingErrorValidationData()
		{
			return new List<ReferenceValidationData>
			{
				new ReferenceValidationData("Ref-101", 0, "#Dispensing", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", "http://www.w3.org/2001/04/xmlenc#sha512",
					null, "qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU=".ToBytes()),
				new ReferenceValidationData("Ref-102", 1, "#id771d57b8-KeyInfo",
					"http://www.w3.org/2001/10/xml-exc-c14n#", "http://www.w3.org/2001/04/xmlenc#sha256",
					"9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg=".ToBytes(), null),
				new ReferenceValidationData("Ref-103", 2, "#id111a1e5c-SignedProperties",
					"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", "http://www.w3.org/2001/04/xmldsig-more#sha384",
					"sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw=".ToBytes(), "sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw=".ToBytes()),
			};
		}

		[Test(Description = "複数署名(検証エラーあり)")]
		public void Generate_MultiSignatureError()
		{
			//TestData(DispPrescription)
			var dispPrescriptionRefs = CreatePrescriptionErrorValidationData();

			var dispPrescriptionSign = new Mock<ISignature>();
			dispPrescriptionSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.DispPrescription);
			dispPrescriptionSign.SetupGet(x => x.ReferenceValidationData).Returns(dispPrescriptionRefs);

			Assert.That(dispPrescriptionSign.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			//TestData(Dispensing)
			var dispensingRefs = CreateDispensingErrorValidationData();

			var dispensingSign = new Mock<ISignature>();
			dispensingSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			dispensingSign.SetupGet(x => x.ReferenceValidationData).Returns(dispensingRefs);

			Assert.That(dispensingSign.Object.ReferenceValidationData.Count, Is.EqualTo(3));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Dispensing, "参照データ", 1, "Reference要素", "Ref-101", "DigestValueのデコードに失敗しました。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 1, "DigestMethod要素", "Ref-101", null),

				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Dispensing, "参照データ", 2, "Reference要素", "Ref-102", "ハッシュ値の計算に失敗しました。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 2, "DigestMethod要素", "Ref-102", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 3, "Reference要素", "Ref-103", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "参照データ", 3, "DigestMethod要素", "Ref-103", null),


				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.DispPrescription, "参照データ", 1, "Reference要素", "Ref-001", "計算したハッシュ値とDigestValueの値が一致しません。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 1, "DigestMethod要素", "Ref-001", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 2, "Reference要素", "Ref-002", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 2, "DigestMethod要素", "Ref-002", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "参照データ", 3, "Reference要素", "Ref-003", null),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.DispPrescription, "参照データ", 3, "DigestMethod要素", "Ref-003", "サポートされていないダイジェストアルゴリズムが指定されています。"),

			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ReferenceReporter(config);

			//Act1(DispPrescription)
			var actual = target.Generate(dispPrescriptionSign.Object, result);

			TestContext.WriteLine("(調剤済み処方)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert1
			AssertPrescriptionError(actual);

			//Act2(Dispensing)
			actual = target.Generate(dispensingSign.Object, result);

			TestContext.WriteLine("(調剤)参照データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert2
			AssertDispensingError(actual);

		}

		private static void AssertDispensingError(ReferenceReportList actual)
		{
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count, Is.EqualTo(3));
			});

			//Ref-101
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Ref-101"));
				Assert.That(report.Uri, Is.EqualTo("#Dispensing"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.DigestValue, Is.Null);
				Assert.That(report.CalculatedDispValue, Is.EqualTo("qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU="));

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("DigestValueのデコードに失敗しました。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-102
			report = actual.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("Ref-102"));
				Assert.That(report.Uri, Is.EqualTo("#id771d57b8-KeyInfo"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-256"));
				Assert.That(report.DigestValue, Is.EqualTo("9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg="));
				Assert.That(report.CalculatedDispValue, Is.Null);

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Message, Is.EqualTo("ハッシュ値の計算に失敗しました。"));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Ref-103
			report = actual.Items.ElementAt(2);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(3));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("Ref-103"));
				Assert.That(report.Uri, Is.EqualTo("#id111a1e5c-SignedProperties"));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N(with comments)"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-384"));
				Assert.That(report.DigestValue, Is.EqualTo("sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw="));
				Assert.That(report.CalculatedDispValue, Is.Null); //同値の場合はnullになる

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "Reference要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "DigestMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

		}
	}
}
