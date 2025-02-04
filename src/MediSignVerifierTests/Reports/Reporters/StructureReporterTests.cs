using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class StructureReporterTests
	{
		[Test(Description = "スキーマ検証失敗")]
		public void Generate_SchemaError()
		{
			var config = new ReportConfig();
			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.None, "署名構造", "XMLスキーマ", "", "要素Aがありません"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.None, "署名構造", "XMLスキーマ", "", "要素Bがありません"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var target = new StructureReporter(config);
			var actual = target.Generate(SignatureSourceType.None, result);
			TestContext.WriteLine("None:");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.Where(m => m.ItemName == "XMLスキーマ" && m.Status == VerificationStatus.INVALID).Count, Is.EqualTo(2));
			});
		}

		[Test(Description = "単一署名")]
		public void Generate_SingleSignature()
		{
			var sourceType = SignatureSourceType.Prescription;
			var config = new ReportConfig();

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, sourceType, "署名構造", "検証内容１", "id-001", null),
				new VerificationResultItem(VerificationStatus.VALID, sourceType, "署名構造", "検証内容２", "id-002", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var target = new StructureReporter(config);
			var actual = target.Generate(sourceType, result);
			TestContext.WriteLine("Prescription:");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容１")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容２")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "複数署名")]
		public void Generate_MultiSignature()
		{
			var config = new ReportConfig();

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "署名構造", "検証内容１", "id-001", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "署名構造", "検証内容２", "id-002", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "署名構造", "検証内容１", "id-101", null),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.DispPrescription, "署名構造", "検証内容２", "id-102", "error!"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var target = new StructureReporter(config);

			//Dispensing
			var actual = target.Generate(SignatureSourceType.Dispensing, result);
			TestContext.WriteLine("Dispensing:");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容１")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容２")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//DispPrescription
			actual = target.Generate(SignatureSourceType.DispPrescription, result);
			TestContext.WriteLine("DispPrescription:");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容１")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "検証内容２")?.Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}

	}
}
