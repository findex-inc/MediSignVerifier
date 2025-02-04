using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class ArchiveTimeStampReporterTests
	{
		[Test(Description = "ValidationDataへの変換エラー")]
		public void Generate_Error()
		{
			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "アーカイブタイムスタンプ", -1, "", "", "不明なエラー"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ArchiveTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("アーカイブタイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Message, Is.EqualTo("不明なエラー"));
				Assert.That(actual.Items.Count, Is.EqualTo(0));
			});
		}

		private readonly SignatureTimeStampVerifierTests.Fixtures _timestampFixtures = new SignatureTimeStampVerifierTests.Fixtures();

		readonly string _targetValueBase64 = @"PHhzOlNpZ25hdHVyZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBJZD0iaWQwMGZhYzIzYy1TaWduYXR1cmVWYWx1ZSI+Q0UyakxEQndFYzFOSlJhb09aL1B2VGhJUjdFdU5yZzVVNU9raFhpcTdUK0E2RXJiMnd2K2xjYm5aV2ZtdGRlbTFCYUt6Y0k5a2syagpZcEc1bkc5c0lKTXE1K0FHakRzeloxMG1rc2h6ZzlMNDRGY2duQjRUZ1ZEMzJVNm8zNjFscVlYVWdJUEhwRDh1SU5HTnZNVmpSZENQCnE2MjA3ZXV2akxQMzVXeWYySGN2OWRtbVdTaUpKUTF5NlVkejZFS3hzczdiU0t4WlV3akFyZ2ZvV1czMUxvTlVGWnd0NHF3eUl1QjUKUmZLc0RhL2JpM3NqMWQ1QkdSRFZibW1DOEVZemN3anZYbGNSekJoNWE0cVJUNTQ5YWxjQWlTdTdjZGhUaFFjNFBONHlIaXNrbjU4ZApPK0xheDd5TjRQa245dkplYUdodFQ2V0YycGdGZ1g1R0Yyb2pIQT09CjwveHM6U2lnbmF0dXJlVmFsdWU+";
		readonly string _targetValueErrorBase64 = "QXRzQ2FsY3VsYXRlZFZhbHVlU2FtcGxl";

		private TimeStampData CreateTimeStampData(DateTime validDate, DateTime genTime, bool isValid)
		{
			return new TimeStampData
			{
				ValidDate = validDate,
				GenTime = genTime,
				HashAlgorithm = "SHA-512",
				HashValue = _targetValueBase64.ToBytes(),
				CalculatedValue = isValid ? _targetValueBase64.ToBytes() : _targetValueErrorBase64.ToBytes(),
				SignerDigestAlgorithm = "SHA-384",
				SignerSignatureAlgorithm = "SHA-384WithRSA",
				MessageDigestValue = "c2lnbmVySW5mby5zaWduZWRBdHRyc+OBriBNZXNzYWdlRGlnZXN0".ToBytes(),
				MessageCalculatedValue = "c2lnbmVySW5mby5zaWduZWRBdHRyc+OBriBNZXNzYWdlRGlnZXN0".ToBytes(),
				CertificateDigestAlgorithm = "SHA-1",
				CertificateHashValue = "U2lnbmluZ0NlcnRpZmljYXRl44Gr5ZCr44G+44KM44KL44OA44Kk44K444Kn44K544OI44Ki44Or44K044Oq44K644Og".ToBytes(),
				CertificateCalculatedValue = "U2lnbmluZ0NlcnRpZmljYXRl44Gr5ZCr44G+44KM44KL44OA44Kk44K444Kn44K544OI44Ki44Or44K044Oq44K644Og".ToBytes(),
			};
		}

		[Test(Description = "単一アーカイブタイムスタンプ")]
		public void Generate_SingleATS()
		{
			var (tsaCert, pathData, trust, _) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), true);

			var data = new TimeStampValidationData("ATS-001",
				null,
				"http://www.w3.org/TR/2001/REC-xml-c14n-20010315",
				null,
				tsaCert,
				pathData,
				Enumerable.Empty<TimeStampConvertException>());

			var validDate = DateTime.UtcNow;
			var genTime = new DateTime(2023, 3, 1, 10, 00, 00, DateTimeKind.Utc);

			var tsData = CreateTimeStampData(validDate, genTime, true);
			data.SetTimeStampData(tsData);

			var archiveTimeStamps = new List<ArchiveTimeStampValidationData>
			{
				new ArchiveTimeStampValidationData("ATS-001", 0, data, null),
			};

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			signature.SetupGet(x => x.ArchiveTimeStampValidationData).Returns(archiveTimeStamps);

			Assert.That(signature.Object.ArchiveTimeStampValidationData.Count, Is.EqualTo(1));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 1, "タイムスタンプトークン", "ATS-001", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 1, "MessageImprint値", "ATS-001", null)
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ArchiveTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(調剤)アーカイブタイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Items.Count, Is.EqualTo(1));
			});

			//Assert(ATS1)
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("ATS-001"));
				Assert.That(report.CalculatedDispValue, Is.Null);

				AssertCommon(report, validDate, genTime);
			});
		}

		[Test(Description = "複数アーカイブタイムスタンプ")]
		public void Generate_MultiATS()
		{
			var validDate1 = DateTime.UtcNow.AddYears(-5);
			var genTime1 = DateTime.UtcNow.AddYears(-5).AddDays(-3);

			var (tsaCert1, pathData1, trust1, _) = _timestampFixtures.GetTestTSAData(validDate1, _targetValueBase64.ToBytes(), true);

			var data1 = new TimeStampValidationData("ATS-001",
				null,
				"http://www.w3.org/TR/2001/REC-xml-c14n-20010315",
				null,
				tsaCert1,
				pathData1,
				Enumerable.Empty<TimeStampConvertException>());

			data1.SetTimeStampData(CreateTimeStampData(validDate1, genTime1, true));

			var validDate2 = DateTime.UtcNow;
			var genTime2 = DateTime.UtcNow.AddDays(-3);

			var (tsaCert2, pathData2, trust2, _) = _timestampFixtures.GetTestTSAData(validDate2, _targetValueBase64.ToBytes(), true);

			var data2 = new TimeStampValidationData("ATS-002",
				null,
				"http://www.w3.org/TR/2001/REC-xml-c14n-20010315",
				null,
				tsaCert2,
				pathData2,
				Enumerable.Empty<TimeStampConvertException>());

			data2.SetTimeStampData(CreateTimeStampData(validDate2, genTime2, false));

			var archiveTimeStamps = new List<ArchiveTimeStampValidationData>
			{
				new ArchiveTimeStampValidationData("ATS-001", 0, data1, null),
				new ArchiveTimeStampValidationData("ATS-002", 1, data2, null),
			};

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			signature.SetupGet(x => x.ArchiveTimeStampValidationData).Returns(archiveTimeStamps);

			Assert.That(signature.Object.ArchiveTimeStampValidationData.Count, Is.EqualTo(2));

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 1, "タイムスタンプトークン", "ATS-001", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 1, "MessageImprint値", "ATS-001", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 2, "タイムスタンプトークン", "ATS-002", null),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Dispensing, "アーカイブタイムスタンプ", 2, "MessageImprint値", "ATS-002", "計算したハッシュ値とhashMessageの値が一致しません。")

			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new ArchiveTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(調剤)アーカイブタイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count, Is.EqualTo(2));
			});

			//ATS-001
			var report = actual.Items.ElementAt(0);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(1));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.Id, Is.EqualTo("ATS-001"));
				Assert.That(report.CalculatedDispValue, Is.Null);
				AssertCommon(report, validDate1, genTime1);

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//ATS-002
			report = actual.Items.ElementAt(1);

			Assert.Multiple(() =>
			{
				Assert.That(report.Index, Is.EqualTo(2));
				Assert.That(report.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.Id, Is.EqualTo("ATS-002"));
				Assert.That(report.CalculatedDispValue, Is.EqualTo(_targetValueErrorBase64));
				AssertCommon(report, validDate2, genTime2);

				Assert.That(report.ResultItems.Count, Is.EqualTo(2));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(report.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Message, Is.EqualTo("計算したハッシュ値とhashMessageの値が一致しません。"));
			});
		}

		private void AssertCommon(ArchiveTimeStampReport report, DateTime validDate, DateTime genTime)
		{
			Assert.Multiple(() =>
			{
				Assert.That(report.TsaCertificateBaseTime, Is.EqualTo(validDate));
				Assert.That(report.TimeStampGenTime, Is.EqualTo(genTime));
				Assert.That(report.C14nAlgorithm, Is.EqualTo("C14N"));
				Assert.That(report.DigestAlgorithm, Is.EqualTo("SHA-512"));
				Assert.That(report.DigestValue, Is.EqualTo(_targetValueBase64));

				Assert.That(report.TsaSignatureInfo.DigestAlgorithm, Is.EqualTo("SHA-384"));
				Assert.That(report.TsaSignatureInfo.SignatureAlgorithm, Is.EqualTo("SHA-384WithRSA"));
				Assert.That(report.TsaSignatureInfo.DigestValue, Is.EqualTo("c2lnbmVySW5mby5zaWduZWRBdHRyc+OBriBNZXNzYWdlRGlnZXN0"));
				Assert.That(report.TsaSignatureInfo.CalculatedDispValue, Is.Null);

				Assert.That(report.TsaCertificateInfo.DigestAlgorithm, Is.EqualTo("SHA-1"));
				Assert.That(report.TsaCertificateInfo.DigestValue, Is.EqualTo("U2lnbmluZ0NlcnRpZmljYXRl44Gr5ZCr44G+44KM44KL44OA44Kk44K444Kn44K544OI44Ki44Or44K044Oq44K644Og"));
				Assert.That(report.TsaCertificateInfo.CalculatedDispValue, Is.Null);

			});
		}
	}
}
