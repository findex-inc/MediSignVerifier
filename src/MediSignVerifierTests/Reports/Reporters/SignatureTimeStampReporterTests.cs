using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SignatureTimeStampReporterTests
	{
		private readonly SignatureTimeStampVerifierTests.Fixtures _timestampFixtures = new SignatureTimeStampVerifierTests.Fixtures();
		private readonly CertificatePathValidationDataTests.Fixtures _signingCertFixtures = new CertificatePathValidationDataTests.Fixtures();

		readonly string _targetValueBase64 = @"PHhzOlNpZ25hdHVyZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBJZD0iaWQwMGZhYzIzYy1TaWduYXR1cmVWYWx1ZSI+Q0UyakxEQndFYzFOSlJhb09aL1B2VGhJUjdFdU5yZzVVNU9raFhpcTdUK0E2RXJiMnd2K2xjYm5aV2ZtdGRlbTFCYUt6Y0k5a2syagpZcEc1bkc5c0lKTXE1K0FHakRzeloxMG1rc2h6ZzlMNDRGY2duQjRUZ1ZEMzJVNm8zNjFscVlYVWdJUEhwRDh1SU5HTnZNVmpSZENQCnE2MjA3ZXV2akxQMzVXeWYySGN2OWRtbVdTaUpKUTF5NlVkejZFS3hzczdiU0t4WlV3akFyZ2ZvV1czMUxvTlVGWnd0NHF3eUl1QjUKUmZLc0RhL2JpM3NqMWQ1QkdSRFZibW1DOEVZemN3anZYbGNSekJoNWE0cVJUNTQ5YWxjQWlTdTdjZGhUaFFjNFBONHlIaXNrbjU4ZApPK0xheDd5TjRQa245dkplYUdodFQ2V0YycGdGZ1g1R0Yyb2pIQT09CjwveHM6U2lnbmF0dXJlVmFsdWU+";
		//readonly string _encupslatedTimeStampBase64 = @"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 BMlkLuXc5/v3F7n76Jxhl4sdT84=";


		[Test(Description = "ValidationDataへの変換エラー")]
		public void Generate_Error()
		{
			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "", "", "不明なエラー"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("署名タイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(1));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "")?.Message, Is.EqualTo("不明なエラー"));
			});
		}

		[Test(Description = "単一署名")]
		public void Generate_SingleSignature()
		{
			var (tsaCert, pathData, trust, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), true);

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				pathData,
				Enumerable.Empty<TimeStampConvertException>());

			var validDate = DateTime.UtcNow;
			var genTime = new DateTime(2023, 3, 1, 10, 00, 00, DateTimeKind.Utc);

			var tsData = new TimeStampData
			{
				ValidDate = validDate,
				GenTime = genTime,
				HashAlgorithm = "SHA-256",
				HashValue = new byte[0],
				CalculatedValue = new byte[0],
				SignerDigestAlgorithm = "SHA-512",
				SignerSignatureAlgorithm = "SHA-512WithRSA",
				MessageDigestValue = new byte[0],
				MessageCalculatedValue = new byte[0],
				CertificateDigestAlgorithm = "SHA-1",
				CertificateHashValue = new byte[0],
				CertificateCalculatedValue = new byte[0],
			};

			data.SetTimeStampData(tsData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "タイムスタンプトークン", "sigTS", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "MessageImprint値", "sigTS", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("署名タイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Id, Is.EqualTo("sigTS"));
				Assert.That(actual.TsaCertificateBaseTime, Is.EqualTo(validDate));
				Assert.That(actual.TimeStampGenTime, Is.EqualTo(genTime));
				Assert.That(actual.DigestAlgorithm, Is.EqualTo("SHA-256"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

		}

		[Test(Description = "TSA証明書有効期間エラー")]
		public void Generate_ExpiredError()
		{
			var (tsaCert, pathData, trust, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), false);

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				pathData,
				Enumerable.Empty<TimeStampConvertException>());

			var validDate = DateTime.UtcNow;
			var genTime = new DateTime(2023, 3, 1, 10, 00, 00, DateTimeKind.Utc);

			var tsData = new TimeStampData
			{
				ValidDate = validDate,
				GenTime = genTime,
				HashAlgorithm = "SHA-256",
				HashValue = new byte[0],
				CalculatedValue = new byte[0],
				SignerDigestAlgorithm = "SHA-512",
				SignerSignatureAlgorithm = "SHA-512WithRSA",
				MessageDigestValue = new byte[0],
				MessageCalculatedValue = new byte[0],
				CertificateDigestAlgorithm = "SHA-1",
				CertificateHashValue = new byte[0],
				CertificateCalculatedValue = new byte[0],
			};

			data.SetTimeStampData(tsData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "TSA証明書", "sigTS", "証明書が有効期間内にありません"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "MessageImprint値", "sigTS", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("署名タイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Id, Is.EqualTo("sigTS"));
				Assert.That(actual.TsaCertificateBaseTime, Is.EqualTo(validDate));
				Assert.That(actual.TimeStampGenTime, Is.EqualTo(genTime));
				Assert.That(actual.DigestAlgorithm, Is.EqualTo("SHA-256"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "TSA証明書")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "TSA証明書パス検証エラー")]
		public void Generate_PathError()
		{
			var (_, _, _, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), true);
			var (tsaCert, testValidData, trust) = _signingCertFixtures.GetTestPathValidationData(DateTime.UtcNow);

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				testValidData,
				Enumerable.Empty<TimeStampConvertException>());

			var validDate = DateTime.UtcNow;
			var genTime = new DateTime(2023, 3, 1, 10, 00, 00, DateTimeKind.Utc);

			var tsData = new TimeStampData
			{
				ValidDate = validDate,
				GenTime = genTime,
				HashAlgorithm = "SHA-256",
				HashValue = new byte[0],
				CalculatedValue = new byte[0],
				SignerDigestAlgorithm = "SHA-512",
				SignerSignatureAlgorithm = "SHA-512WithRSA",
				MessageDigestValue = new byte[0],
				MessageCalculatedValue = new byte[0],
				CertificateDigestAlgorithm = "SHA-1",
				CertificateHashValue = new byte[0],
				CertificateCalculatedValue = new byte[0],
			};

			data.SetTimeStampData(tsData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INDETERMINATE, SignatureSourceType.Prescription, "署名タイムスタンプ", "TSA証明書", "sigTS", "上位証明書が見つかりません。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名タイムスタンプ", "MessageImprint値", "sigTS", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureTimeStampReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("署名タイムスタンプ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(actual.Id, Is.EqualTo("sigTS"));
				Assert.That(actual.TsaCertificateBaseTime, Is.EqualTo(validDate));
				Assert.That(actual.TimeStampGenTime, Is.EqualTo(genTime));
				Assert.That(actual.DigestAlgorithm, Is.EqualTo("SHA-256"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "TSA証明書")?.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

		}
	}
}
