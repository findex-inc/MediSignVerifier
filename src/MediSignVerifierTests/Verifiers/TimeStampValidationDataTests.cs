using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Data;

namespace SignatureVerifier.Verifiers
{
	internal class TimeStampValidationDataTests
	{
		//XML要素を正規化してBase64化したもの
		readonly string _targetValueBase64 = @"PHhzOlNpZ25hdHVyZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBJZD0iaWQwMGZhYzIzYy1TaWduYXR1cmVWYWx1ZSI+Q0UyakxEQndFYzFOSlJhb09aL1B2VGhJUjdFdU5yZzVVNU9raFhpcTdUK0E2RXJiMnd2K2xjYm5aV2ZtdGRlbTFCYUt6Y0k5a2syagpZcEc1bkc5c0lKTXE1K0FHakRzeloxMG1rc2h6ZzlMNDRGY2duQjRUZ1ZEMzJVNm8zNjFscVlYVWdJUEhwRDh1SU5HTnZNVmpSZENQCnE2MjA3ZXV2akxQMzVXeWYySGN2OWRtbVdTaUpKUTF5NlVkejZFS3hzczdiU0t4WlV3akFyZ2ZvV1czMUxvTlVGWnd0NHF3eUl1QjUKUmZLc0RhL2JpM3NqMWQ1QkdSRFZibW1DOEVZemN3anZYbGNSekJoNWE0cVJUNTQ5YWxjQWlTdTdjZGhUaFFjNFBONHlIaXNrbjU4ZApPK0xheDd5TjRQa245dkplYUdodFQ2V0YycGdGZ1g1R0Yyb2pIQT09CjwveHM6U2lnbmF0dXJlVmFsdWU+";
		readonly string _encupslatedTimeStampBase64 = @"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 BMlkLuXc5/v3F7n76Jxhl4sdT84=";
		readonly string _signerCertBase64 = @"MIIDqTCCApGgAwIBAgIBJTANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJKUDEcMBoGA1UEChMT U2FtcGxlIE9yZ2FuaXphdGlvbjESMBAGA1UECxMJU2FtcGxlIENBMSQwIgYDVQQDExtUZXN0IENB IGZvciBUaW1lU3RhbXBTZXJ2ZXIwHhcNMTcwNjIxMTAyODEyWhcNMzMxMjMxMTUwMDAwWjBXMQsw CQYDVQQGEwJKUDENMAsGA1UEChMETURJUzEdMBsGA1UECxMURmluYW5jaWFsIERlcGFydG1lbnQx GjAYBgNVBAMTEVRpbWVTdGFtcFNlcnZlcjAxMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC AQEA2JNfR4b84cuqx+Obf2FX5lFK45RoxqvCrjvuNT5vbWZGinnmPqIwgZiiOw7qV2GZCN6l3VuI 87Ds+NE6Fd9APTo7KOCUXnn+Q4IMp11s+++dnq7/n8UDEH/Y/0w4Hgco4WT9r5QWobNCdvmkWpuv p5wm78msw+nGMThROIMQN90uyuId6aSWEZwDcqLRoaIOaY4KXW52w5WOfNjZxw9znHtFTW5QWVOP 2DfS+ET+iwwT8kkOxHj5oYlxThSgBAkA536RtKmZHCcRsdaliNPFBKaiQWP7CrjSGuab/l7e3BoI QA3cPmKCSym45amHgBYZq0WaT/BXNkCM2QXhdpPq6QIDAQABo3IwcDAdBgNVHQ4EFgQUwhY9pQih RxKqVHBuco0PGQvHJtUwHwYDVR0jBBgwFoAUPtzxR5kBmsO6I2N+kGCWAlXEuOIwCQYDVR0TBAIw ADALBgNVHQ8EBAMCBsAwFgYDVR0lAQH/BAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcNAQELBQADggEB AGw6XGu2+k2EqUWx7aSVf8E8c+vMdG3lCuTbPz7Et4WRvUT1NZRbLEBpf8GRC4X+SOs489j+rQ0g jgi4u4Q1aI+NrN5bf1q4OaRkbPwuiWpxYdwmCjaswmZGY83NXrVjqRE1jYnH6cl0gc8O6z3JpxvE 9qgODyHqsTzwy4sIS8V/gJVXyPPLh4wUtfR3aTPvjZgMkCUGSLz4wBdEkwtZKfVGnAmn/BGVlMsp sbuRBOaottZCuOnkOy4V9Y98cG2P7mrYczy+LLoKZ4125yRTAGXCCn1svmPjwMufmaAcY4bHpKpK ZL8+r21bUo/DjhYSRIIEeARFIgTv0m1KnP5AGE0=";
		readonly string[] certs = new string[]
		{
				"MIIDqTCCApGgAwIBAgIBJTANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJKUDEcMBoGA1UEChMT U2FtcGxlIE9yZ2FuaXphdGlvbjESMBAGA1UECxMJU2FtcGxlIENBMSQwIgYDVQQDExtUZXN0IENB IGZvciBUaW1lU3RhbXBTZXJ2ZXIwHhcNMTcwNjIxMTAyODEyWhcNMzMxMjMxMTUwMDAwWjBXMQsw CQYDVQQGEwJKUDENMAsGA1UEChMETURJUzEdMBsGA1UECxMURmluYW5jaWFsIERlcGFydG1lbnQx GjAYBgNVBAMTEVRpbWVTdGFtcFNlcnZlcjAxMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC AQEA2JNfR4b84cuqx+Obf2FX5lFK45RoxqvCrjvuNT5vbWZGinnmPqIwgZiiOw7qV2GZCN6l3VuI 87Ds+NE6Fd9APTo7KOCUXnn+Q4IMp11s+++dnq7/n8UDEH/Y/0w4Hgco4WT9r5QWobNCdvmkWpuv p5wm78msw+nGMThROIMQN90uyuId6aSWEZwDcqLRoaIOaY4KXW52w5WOfNjZxw9znHtFTW5QWVOP 2DfS+ET+iwwT8kkOxHj5oYlxThSgBAkA536RtKmZHCcRsdaliNPFBKaiQWP7CrjSGuab/l7e3BoI QA3cPmKCSym45amHgBYZq0WaT/BXNkCM2QXhdpPq6QIDAQABo3IwcDAdBgNVHQ4EFgQUwhY9pQih RxKqVHBuco0PGQvHJtUwHwYDVR0jBBgwFoAUPtzxR5kBmsO6I2N+kGCWAlXEuOIwCQYDVR0TBAIw ADALBgNVHQ8EBAMCBsAwFgYDVR0lAQH/BAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcNAQELBQADggEB AGw6XGu2+k2EqUWx7aSVf8E8c+vMdG3lCuTbPz7Et4WRvUT1NZRbLEBpf8GRC4X+SOs489j+rQ0g jgi4u4Q1aI+NrN5bf1q4OaRkbPwuiWpxYdwmCjaswmZGY83NXrVjqRE1jYnH6cl0gc8O6z3JpxvE 9qgODyHqsTzwy4sIS8V/gJVXyPPLh4wUtfR3aTPvjZgMkCUGSLz4wBdEkwtZKfVGnAmn/BGVlMsp sbuRBOaottZCuOnkOy4V9Y98cG2P7mrYczy+LLoKZ4125yRTAGXCCn1svmPjwMufmaAcY4bHpKpK ZL8+r21bUo/DjhYSRIIEeARFIgTv0m1KnP5AGE0=",
				"MIIEFzCCAv+gAwIBAgIJAIK9bBWUV3OZMA0GCSqGSIb3DQEBCwUAMGUxCzAJBgNVBAYTAkpQMRww GgYDVQQKExNTYW1wbGUgT3JnYW5pemF0aW9uMRIwEAYDVQQLEwlTYW1wbGUgQ0ExJDAiBgNVBAMT G1Rlc3QgQ0EgZm9yIFRpbWVTdGFtcFNlcnZlcjAeFw0xNzA2MjExMDI4MDZaFw0yNzA2MjkxMDI4 MDZaMGUxCzAJBgNVBAYTAkpQMRwwGgYDVQQKExNTYW1wbGUgT3JnYW5pemF0aW9uMRIwEAYDVQQL EwlTYW1wbGUgQ0ExJDAiBgNVBAMTG1Rlc3QgQ0EgZm9yIFRpbWVTdGFtcFNlcnZlcjCCASIwDQYJ KoZIhvcNAQEBBQADggEPADCCAQoCggEBAM7FtYwz28X2bandNM4hJk3zsObkM/apz7eyFl7Jbd6O v3ZmDlrjv9ADHaOMcMi3VwqGdcHg2GLbJ+ev2iI/7oPDRWA4CbUZq5XSsHg/aXSWfDzNKV2fMbjq j8Y0AZx9HKDonCRTimRhhyqvk4wXj1FqzNxbVyMlJkr6eddOXZ/s4inxt/F7HZ32Lgdiy62JA8Ji yUTgwXvOHmtyPjTGvVvBurkWfKgklxHDPryupM7VMsA0Ii32QZWAaTBHfw68bCotQJTmEl9qgAIs ZUgLj0Q7iF+r67zfVkcMKecVkPPNtyvd1MaAlmJb+w7rrKwAVkBqAkLwMjaQ1MMNJ+b+eYcCAwEA AaOByTCBxjAdBgNVHQ4EFgQUPtzxR5kBmsO6I2N+kGCWAlXEuOIwgZcGA1UdIwSBjzCBjIAUPtzx R5kBmsO6I2N+kGCWAlXEuOKhaaRnMGUxCzAJBgNVBAYTAkpQMRwwGgYDVQQKExNTYW1wbGUgT3Jn YW5pemF0aW9uMRIwEAYDVQQLEwlTYW1wbGUgQ0ExJDAiBgNVBAMTG1Rlc3QgQ0EgZm9yIFRpbWVT dGFtcFNlcnZlcoIJAIK9bBWUV3OZMAsGA1UdDwQEAwIBBjANBgkqhkiG9w0BAQsFAAOCAQEAJH9B QOXJQGP91iKWHWNYWAx7pPGDYIw2HV9HvuWqfz9PDTq7cr2CDl3WltwZzp0xe7vz9bCggmRrodoN GRUDG3aYXHgWfGtZpviJTKa0k65eCCFWIBz9K69sLkLtHUs/GqpcVon6XIKCWv2UcGhmFKT6slx7 3f3V4+MuDDsSUc3k2kNMqBBo43fy7Kv8hY6z6XDcjX7VQAlgeS2uddLb84eMM8ekOVSfZj4CSWGs +J6WHdPj/48Y5hv3Y1dgLf4qIdv187iuSdDB435KwFUuHjewf+jAzbNVMzdzz3pjG0M3ZIXplE2B Q0vYK5wir7YCpFqEj1ffA/emGE2FoWJNSA=="
		};
		readonly string[] crls = new string[]
		{
				"MIIBvzCBqAIBATANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJKUDEcMBoGA1UEChMTU2FtcGxl IE9yZ2FuaXphdGlvbjESMBAGA1UECxMJU2FtcGxlIENBMSQwIgYDVQQDExtUZXN0IENBIGZvciBU aW1lU3RhbXBTZXJ2ZXIXDTE3MDYyMTEwMjgyN1oXDTI3MDYxOTEwMjgyN1qgDzANMAsGA1UdFAQE AgIBADANBgkqhkiG9w0BAQsFAAOCAQEADuk19hLbnRb5UibKKx38CNVdaDSPM8vkX08t9VvDc2bM Lq2fJ4sd3hsbciSJYFYxG2q95hYv8WUOQ2/yuMQHc69X+BE8Rs2TqcjZrALXy9gw9uIthHDPsRNe oNbqCrNALfPnMldgJI2jX2UnFamEaUZn7T4PQ0PspAKkQcAxdeh0ofUH1VPkFcdJvgIZdYuZqzK8 LxLkFsHFHjl3NVCHUXm8DWJx2SVGtJdTqtAp/1dO4MtggXZAzFZ59geR1IA7dGiz1zQsvqK5S3s2 FLyUbIu6QAzeda46/3NJPxnfwk93RRdKRHa+kr3EQgz7UVw8JZ7FOzELVuoqLTj6RHMNbA=="
		};

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - VALID")]
		public void Validate_All()
		{
			var data = new TimeStampValidationData("id97e936cc",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				_targetValueBase64.ToBytes(),
				new CertificateData("cert", _signerCertBase64.ToBytes()),
				new CertificatePathValidationData("timestamp", certs.Select(m => m.ToBytes()), crls.Select(m => m.ToBytes())),
				null);

			data.Validate(DateTime.UtcNow, null, out TimeStampValidationExceptions errors);
		}

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - データ構造 - VALID")]
		public void Validate_Structure()
		{
			var data = new TimeStampValidationData("idbc9e4f38",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				new byte[0],
				new CertificateData("cert", new byte[0]),
				new CertificatePathValidationData("timestamp", Enumerable.Empty<byte[]>(), Enumerable.Empty<byte[]>()),
				null);

			data.Validate(DateTime.UtcNow, null, out _);
		}

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - データ構造 - データ構造の正当性確認 - INVALID")]
		public void Validate_Structure_Error()
		{
			var data = new TimeStampValidationData("errorTS",
				new byte[0],
				null,
				new byte[0],
				new CertificateData("cert", new byte[0]),
				new CertificatePathValidationData("timestamp", Enumerable.Empty<byte[]>(), Enumerable.Empty<byte[]>()),
				null);

			var ex = Assert.Throws<TimeStampValidationException>(() => data.Validate(DateTime.UtcNow, null, out _));

			Assert.Multiple(() =>
			{
				//構造確認系でエラーがあった場合はThrowされる
				Assert.That(ex, Is.Not.Null);
				Assert.That(ex.ItemName, Is.EqualTo("データ構造の正当性確認"));
				Assert.That(ex.Message, Is.EqualTo("データ構造が不正です。"));
			});
		}

		//[Test(Description = "5.6.1 タイムスタンプの検証要件 - データ構造 - CMSデータ形式の確認 - INVALID")]
		//public void Validate_Structure_SignedDataError()
		//{
		//	//ContentTypeがsigned-dataの識別子であること == false
		//	var data = new TimeStampValidationData("errorTS",
		//		_encupslatedTimeStampBase64.ToBytes(),
		//		new byte[0],
		//		new CertificateData("cert", new byte[0]),
		//		Enumerable.Empty<byte[]>(),
		//		Enumerable.Empty<byte[]>(),
		//		null, null);

		//	TimeStampValidationException structureError = null;

		//	try {
		//		data.Validate(DateTime.UtcNow, out _);
		//	}
		//	catch (TimeStampValidationException ex) {
		//		structureError = ex;
		//	}

		//	Assert.Multiple(() =>
		//	{
		//		//構造確認系でエラーがあった場合はThrowされる
		//		Assert.That(structureError, Is.Not.Null);
		//		Assert.That(structureError.ItemName, Is.EqualTo("CMSデータ形式の確認"));
		//		Assert.That(structureError.Message, Does.StartWith("ContentTypeが不正です。"));
		//	});
		//}

		//[Test(Description = "5.6.1 タイムスタンプの検証要件 - データ構造 - 署名対象データ形式の確認 - INVALID")]
		//public void Validate_Structure_TSTInfoError()
		//{
		//	//eContentTypeがTSTInfoのオブジェクト識別子であること == false
		//	var data = new TimeStampValidationData("errorTS",
		//		_encupslatedTimeStampBase64.ToBytes(),
		//		new byte[0],
		//		new CertificateData("cert", new byte[0]),
		//		Enumerable.Empty<byte[]>(),
		//		Enumerable.Empty<byte[]>(),
		//		null, null);

		//	TimeStampValidationException structureError = null;

		//	try {
		//		data.Validate(DateTime.UtcNow, out _);
		//	}
		//	catch (TimeStampValidationException ex) {
		//		structureError = ex;
		//	}

		//	Assert.Multiple(() =>
		//	{
		//		//構造確認系でエラーがあった場合はThrowされる
		//		Assert.That(structureError, Is.Not.Null);
		//		Assert.That(structureError.ItemName, Is.EqualTo("署名対象データ形式の確認"));
		//		Assert.That(structureError.Message, Does.StartWith("eContentTypeが不正です。"));
		//	});
		//}

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - TSA証明書 - INVALID")]
		public void Validate_TsaCertificateConvertError()
		{
			//変換でエラーになるパターン
			var data = new TimeStampValidationData("errorCert",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				_targetValueBase64.ToBytes(),
				new CertificateData("cert", new byte[0]),
				new CertificatePathValidationData("timestamp", Enumerable.Empty<byte[]>(), Enumerable.Empty<byte[]>()),
				new List<TimeStampConvertException> { new TimeStampConvertException("errorCert", "TSA証明書群のデコードに失敗しました。") });

			data.Validate(DateTime.UtcNow, null, out TimeStampValidationExceptions errors);

			Assert.Multiple(() =>
			{
				Assert.That(errors.Any(m => m.ItemName == "TSA証明書"), Is.True);
				Assert.That(errors.Any(m => m.Message == "TSA証明書群のデコードに失敗しました。"), Is.True);
			});
		}

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - TSA証明書 - INVALID")]
		public void Validate_TsaCertificateError()
		{
			//証明書標準検証でエラーになるパターン
			var data = new TimeStampValidationData("errorCert",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				_targetValueBase64.ToBytes(),
				new CertificateData("cert", new byte[0]),
				new CertificatePathValidationData("timestamp", Enumerable.Empty<byte[]>(), Enumerable.Empty<byte[]>()),
				null);

			data.Validate(DateTime.UtcNow, null, out TimeStampValidationExceptions errors);

			Assert.Multiple(() =>
			{
				Assert.That(errors.Any(m => m.ItemName == "TSA証明書"), Is.True);
				Assert.That(errors.Any(m => m.Message != "TSA証明書群のデコードに失敗しました。" && m.Message != "TSA証明書失効情報群のデコードに失敗しました。"), Is.True);
			});
		}

		//[Test(Description = "5.6.1 タイムスタンプの検証要件 - TSA証明書 - 鍵拡張利用目的の確認 - INVALID")]
		//public void Validate_TsaCertificateExtKeyUsageError()
		//{
		//	//鍵拡張利用目的にタイムスタンプが設定されていない
		//	var data = new TimeStampValidationData("errorCert",
		//		_encupslatedTimeStampBase64.ToBytes(),
		//		new byte[0],
		//		new CertificateData("cert", new byte[0]),
		//		Enumerable.Empty<byte[]>(),
		//		Enumerable.Empty<byte[]>(),
		//		null, null);

		//	var result = true;
		//	TimeStampValidationExceptions errors = null;
		//	try {
		//		data.Validate(DateTime.UtcNow, out errors);
		//	}
		//	catch (Exception ex) {
		//		result = false;
		//	}

		//	Assert.Multiple(() =>
		//	{
		//		Assert.That(result, Is.True);
		//		Assert.That(errors.Any(m => m.ItemName == "TSA証明書"), Is.True);
		//		Assert.That(errors.Any(m => m.Message == "鍵拡張利用目的にid-kp-timeStamping(critical)が存在しません。"), Is.True);
		//	});
		//}

		//[Test(Description = "5.6.1 タイムスタンプの検証要件 - TSA証明書 - 鍵利用目的の確認(optional) - INVALID")]
		//public void Validate_TsaCertificateKeyUsageError()
		//{
		//	//鍵利用目的にdigitalSignatureもnonRepdiation も設定されていない
		//	var data = new TimeStampValidationData("errorCert",
		//		_encupslatedTimeStampBase64.ToBytes(),
		//		new byte[0],
		//		new CertificateData("cert", new byte[0]),
		//		Enumerable.Empty<byte[]>(),
		//		Enumerable.Empty<byte[]>(),
		//		null, null);

		//	var result = true;
		//	TimeStampValidationExceptions errors = null;
		//	try {
		//		data.Validate(DateTime.UtcNow, out errors);
		//	}
		//	catch (Exception ex) {
		//		result = false;
		//	}

		//	Assert.Multiple(() =>
		//	{
		//		Assert.That(result, Is.True);
		//		Assert.That(errors.Any(m => m.ItemName == "TSA証明書"), Is.True);
		//		Assert.That(errors.Any(m => m.Message == "鍵利用目的にdigitalSignature/nonRepdiationが存在しません。"), Is.True);
		//	});
		//}

		//[Test(Description = "5.6.1 タイムスタンプの検証要件 - TSAの署名 - INVALID")]
		//public void Validate_TsaSignatureError()
		//{

		//}

		[Test(Description = "5.6.1 タイムスタンプの検証要件 - MessageImprint値 - ハッシュ - INVALID")]
		public void Validate_HashMessageError()
		{
			var targetValueErrorBase64 = @"PHhzOlNpZ25hdHVyZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBJZD0iaWQyMTg5MzRmZS1TaWduYXR1cmVWYWx1ZSI+YWRQVlBNSnNCVlFLVUEyRUhRNlhDcXNKcjFIZDYzTEVSUVNWeWI1K3NoZFpTV0s4VkR0V2dSd3Q1blVPTDlUanovL1dWcllYYmhzeQpiejZWNXVzY1Y5eUNqUkdSdmpSYmxwSytrak80QnBZK3l1UDBVR0NRRDJJTTYxczBHUEVkYUxiUGpTOUl1dmlrNlkrV25ycnZZb0NVClZ0anpEcFdTaHc0WmJTWWtjN3M1NGhYWTIyVUVtTTVDOTUzRUJMVkV0RzFtT2IrNjB2R0MwZ0hpaVhha0ZISUlmazhoM2E0YUpKQkIKalcyTUNDc054eTA3Y1kxYzVFUXI5dlRLdGdDVGZVTVAyQ05wam83Z1kyd0N5U2R0RTFiYldzNjBoZitiRldHdTYrUnA0ZWM1Z0RMWApxUk12eWFEdDdvWXdmUzA2TmlzaFVRei9BbEpQSHJwR2VtaU1rQT09CjwveHM6U2lnbmF0dXJlVmFsdWU+";

			var data = new TimeStampValidationData("errorHash",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				targetValueErrorBase64.ToBytes(),
				new CertificateData("cert", _signerCertBase64.ToBytes()),
				new CertificatePathValidationData("timestamp", certs.Select(m => m.ToBytes()), crls.Select(m => m.ToBytes())),
				null);

			data.Validate(DateTime.UtcNow, null, out TimeStampValidationExceptions errors);

			Assert.Multiple(() =>
			{
				Assert.That(errors.Any(m => m.ItemName == "MessageImprint値"), Is.True);
				Assert.That(errors.Any(m => m.Message == "計算したハッシュ値とhashMessageの値が一致しません。"), Is.True);
			});
		}
	}
}
