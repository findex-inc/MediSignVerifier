using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Properties;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Verifiers
{
	public partial class SignatureTimeStampVerifierTests
	{
		private readonly Fixtures _timestampFixtures = new Fixtures();
		private readonly CertificatePathValidationDataTests.Fixtures _signingCertFixtures = new CertificatePathValidationDataTests.Fixtures();

		readonly string _targetValueBase64 = @"PHhzOlNpZ25hdHVyZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiBJZD0iaWQwMGZhYzIzYy1TaWduYXR1cmVWYWx1ZSI+Q0UyakxEQndFYzFOSlJhb09aL1B2VGhJUjdFdU5yZzVVNU9raFhpcTdUK0E2RXJiMnd2K2xjYm5aV2ZtdGRlbTFCYUt6Y0k5a2syagpZcEc1bkc5c0lKTXE1K0FHakRzeloxMG1rc2h6ZzlMNDRGY2duQjRUZ1ZEMzJVNm8zNjFscVlYVWdJUEhwRDh1SU5HTnZNVmpSZENQCnE2MjA3ZXV2akxQMzVXeWYySGN2OWRtbVdTaUpKUTF5NlVkejZFS3hzczdiU0t4WlV3akFyZ2ZvV1czMUxvTlVGWnd0NHF3eUl1QjUKUmZLc0RhL2JpM3NqMWQ1QkdSRFZibW1DOEVZemN3anZYbGNSekJoNWE0cVJUNTQ5YWxjQWlTdTdjZGhUaFFjNFBONHlIaXNrbjU4ZApPK0xheDd5TjRQa245dkplYUdodFQ2V0YycGdGZ1g1R0Yyb2pIQT09CjwveHM6U2lnbmF0dXJlVmFsdWU+";
		readonly string _encupslatedTimeStampBase64 = @"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 BMlkLuXc5/v3F7n76Jxhl4sdT84=";

		[Test(Description = "(処方)5.6.2 署名タイムスタンプの検証要件 - VALID")]
		public void VerifyPrescriptionSignatureTimeStamp()
		{
			var doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription-004_01.xml");
			TestData.SetESLevel(ESLevel.XL, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			var verificationTime = DateTime.Now;

			//Act.
			var result = target.Verify(data, verificationTime);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.Count(), Is.EqualTo(2));
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("タイムスタンプトークン"));
				Assert.That(item.MappedItem, Is.EqualTo("idbc9e4f38"));
				Assert.That(item.Message, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("MessageImprint値"));
				Assert.That(item.MappedItem, Is.EqualTo("idbc9e4f38"));
				Assert.That(item.Message, Is.Null);
			});

			var signature = data.Signatures.ElementAt(0);

			//検証基準時刻(検証実行時刻)
			Assert.That(signature.SignatureTimeStampValidationData.TimeStampData.ValidDate, Is.EqualTo(verificationTime.ToUniversalTime()));
		}

		[Test(Description = "(調剤)5.6.2 署名タイムスタンプの検証要件 - VALID")]
		public void VerifyDispensingSignatureTimeStamp()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing-005_03.xml");
			var signature = data.Signatures.Where(m => m.SourceType == SignatureSourceType.Dispensing).First();

			TestData.SetESLevel(ESLevel.A, signature);

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			var verificationTime = DateTime.Now;

			//Act.
			var result = target.Verify(data, verificationTime);
			target.VerifiedEvent -= handler;

			TestContext.WriteLine($"OldestATSGenTime:{signature.OldestArchiveTimeStampGenTime}");

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.Count(), Is.EqualTo(4));
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("タイムスタンプトークン"));
				Assert.That(item.MappedItem, Is.EqualTo("id96e0a365"));
				Assert.That(item.Message, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("MessageImprint値"));
				Assert.That(item.MappedItem, Is.EqualTo("id96e0a365"));
				Assert.That(item.Message, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(2);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("タイムスタンプトークン"));
				Assert.That(item.MappedItem, Is.EqualTo("idbc9e4f38"));
				Assert.That(item.Message, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(3);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.ItemName, Is.EqualTo("MessageImprint値"));
				Assert.That(item.MappedItem, Is.EqualTo("idbc9e4f38"));
				Assert.That(item.Message, Is.Null);
			});

			//検証基準時刻(調剤)：最初のATS時刻
			var signature1 = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			Assert.That(signature1.SignatureTimeStampValidationData.TimeStampData.ValidDate, Is.EqualTo(signature1.OldestArchiveTimeStampGenTime));

			//検証基準時刻(調剤内処方)：調剤の署名TS時刻
			var signature2 = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.DispPrescription);
			Assert.That(signature2.SignatureTimeStampValidationData.TimeStampData.ValidDate, Is.EqualTo(signature1.SignatureTimeStampValidationData.TimeStampData.GenTime));
		}


		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - VALID")]
		public void VerifySignatureTimeStampWithMock()
		{
			var (tsaCert, pathData, trust, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), true);

			var config = new VerificationConfig();

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				pathData,
				Enumerable.Empty<TimeStampConvertException>());

			var signature = new Mock<ISignature>();
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.XL);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(doc.Object, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.Any(m => m.ItemName == "タイムスタンプトークン" && m.MappedItem == "sigTS" && m.Status == VerificationStatus.VALID), Is.True);
				Assert.That(result.Items.Any(m => m.ItemName == "MessageImprint値" && m.MappedItem == "sigTS" && m.Status == VerificationStatus.VALID), Is.True);
			});
		}

		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - TSA証明書 - 有効期間チェック - INVALID")]
		public void VerifySignatureTimeStampExpiredError()
		{
			var (tsaCert, pathData, trust, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), false);

			var config = new VerificationConfig();

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				pathData,
				Enumerable.Empty<TimeStampConvertException>());

			var signature = new Mock<ISignature>();
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.XL);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(doc.Object, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.Any(m => m.ItemName == "TSA証明書" && m.MappedItem == "sigTS" && m.Message.StartsWith("証明書が有効期間内にありません。")), Is.True);
			});
		}

		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - TSA証明書 - パス検証 - INDETERMINATE")]
		public void VerifySignatureTimeStampPathError()
		{
			var config = new VerificationConfig();

			//BouncyCastleでタイムスタンプを作る場合、証明書の用途が不正なものを使うことはできない。
			var (tsaCert, _, _, timeStamp) = _timestampFixtures.GetTestTSAData(DateTime.UtcNow, _targetValueBase64.ToBytes(), true);
			var (_, testValidData, trust) = _signingCertFixtures.GetTestPathValidationData(DateTime.UtcNow);

			var data = new TimeStampValidationData("sigTS",
				timeStamp,
				null,
				_targetValueBase64.ToBytes(),
				tsaCert,
				testValidData,
				Enumerable.Empty<TimeStampConvertException>());

			var signature = new Mock<ISignature>();
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.XL);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(doc.Object, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(result.Items.Any(m => m.ItemName == "TSA証明書" && m.MappedItem == "sigTS"
					&& m.Message == "上位証明書が見つかりません。" && m.Status == VerificationStatus.INDETERMINATE), Is.True);
			});
		}

		//検証基準時刻
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - 検証基準時刻 - INVALID")]
		public void VerifyValidDateError()
		{
			var config = new VerificationConfig();

			var data = new TimeStampValidationData("validDate",
				_encupslatedTimeStampBase64.ToBytes(),
				null,
				_targetValueBase64.ToBytes(),
				new CertificateData("tsCert", new byte[0]),
				new CertificatePathValidationData(null, Enumerable.Empty<byte[]>(), Enumerable.Empty<byte[]>()),
				Enumerable.Empty<TimeStampConvertException>());

			var signature = new Mock<ISignature>();
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.A);
			signature.SetupGet(m => m.SignatureTimeStampValidationData).Returns(data);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(doc.Object, DateTime.Now);
			target.VerifiedEvent -= handler;

			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "検証基準時刻").Status,
					Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "検証基準時刻").Message,
					Is.EqualTo("検証基準時刻の取得に失敗しました。"));
			});
		}

		//XML解析（タイムスタンプ対象）
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - タイムスタンプ対象データ - 存在チェック - INVALID")]
		public void VerifyStructure_TargetExistsError()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.NotFoundSignatureValue);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "NotFoundSignatureValue.xml");
			TestData.SetESLevel(ESLevel.T, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプ対象データ")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプ対象データ")?.Message, Is.EqualTo("タイムスタンプ対象要素が見つかりませんでした。"));
			});
		}

		//XML解析（タイムスタンプ要素）
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - タイムスタンプ要素 - 存在チェック - INVALID")]
		public void VerifyStructure_TimeStampExistsError()
		{
			//この要素がなければ、そもそも-T以降にはならないはずなんだけど
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.NotFoundTimeStamp);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "NotFoundTimeStamp.xml");
			TestData.SetESLevel(ESLevel.T, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプトークン")?.Message, Is.EqualTo("タイムスタンプの要素が見つかりませんでした。"));
			});
		}

		//XML解析（タイムスタンプ要素）
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - タイムスタンプ要素 - デコード - INVALID")]
		public void VerifyTimeStampBase64Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.TimeStampBase64Error);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "TimeStampBase64Error.xml");
			TestData.SetESLevel(ESLevel.T, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "タイムスタンプトークン")?.Message, Is.EqualTo("タイムスタンプのデコードに失敗しました。"));
			});
		}

		//XML解析（TSA証明書群・失効リスト群）
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - TSA証明書群＆失効リスト群 - デコード - INVALID")]
		public void VerifyCertificatesRevocationsBase64Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.CertsCrlsBase64Error);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "CertsCrlsBase64Error.xml");
			TestData.SetESLevel(ESLevel.XL, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));

				var certError = result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "TSA証明書" && m.Message == "TSA証明書群のデコードに失敗しました。");
				Assert.That(certError, Is.Not.Null);
				Assert.That(certError.Status, Is.EqualTo(VerificationStatus.INVALID));

				var crlError = result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "TSA証明書" && m.Message == "TSA証明書失効情報群のデコードに失敗しました。");
				Assert.That(crlError, Is.Not.Null);
				Assert.That(crlError.Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}

		//タイムスタンプ共通(ハッシュ値)
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - MessageImprint値 - INVALID")]
		public void VerifyMessageImprintError()
		{
			//SignatureValueの値を書き換える
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.SignatureValueHashError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "SignatureValueHashError.xml");
			TestData.SetESLevel(ESLevel.T, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));

				var hashError = result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "MessageImprint値" && m.Message == "計算したハッシュ値とhashMessageの値が一致しません。");
				Assert.That(hashError, Is.Not.Null);
				Assert.That(hashError.Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}

		//タイムスタンプ共通(ハッシュ値)
		[Test(Description = "5.6.2 署名タイムスタンプの検証要件 - MessageImprint値(正規化アルゴリズム改ざん) - INVALID")]
		public void VerifyMessageImprintC14nError()
		{
			//TimeStamp要素にCanocnicalizationMethod要素を追加（Exclusive）
			var doc = TestData.CreateXmlDocument(Resources_SignatureTimeStamp.TimeStampC14nMethodError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "TimeStampC14nMethodError.xml");
			TestData.SetESLevel(ESLevel.T, data.Signatures.First());

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);
			target.VerifiedEvent -= handler;

			TestContext.WriteLine($"TimeStampC14nMethod:{data.Signatures.First().SignatureTimeStampValidationData.CanonicalizationMethod}");
			TestContext.WriteLine($"TimeStampHash:{data.Signatures.First().SignatureTimeStampValidationData.TimeStampData.HashValue.ToHexString()}");
			TestContext.WriteLine($"CalculatedHash:{data.Signatures.First().SignatureTimeStampValidationData.TimeStampData.CalculatedValue.ToHexString()}");

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名タイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));

				var hashError = result.Items.FirstOrDefault(m => m.Source == "署名タイムスタンプ" && m.ItemName == "MessageImprint値" && m.Message == "計算したハッシュ値とhashMessageの値が一致しません。");
				Assert.That(hashError, Is.Not.Null);
				Assert.That(hashError.Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}

	}
}
