using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Properties;
using SignatureVerifier.Verifiers;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXmlTests
	{
		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - (単一) - VALID")]
		public void VerifyArchiveTimeStampFromXml()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing-005_03.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			foreach (var ats in signature.ArchiveTimeStampValidationData) {

				//次の世代のアーカイブタイムスタンプがあれば、その日付を使って検証する
				var nextGenAts = signature.ArchiveTimeStampValidationData.FirstOrDefault(m => m.Index == ats.Index + 1)?.ValidationData.TimeStamp;
				var validDate = nextGenAts?.ToTimeStampToken().TimeStampInfo.GenTime ?? DateTime.Now;

				ats.ValidationData.Validate(validDate, config, out TimeStampValidationExceptions errors);
				TestContext.WriteLine($"ATS Index:{ats.DispIndex}");
				TestContext.WriteLine($"Valid Date:{validDate.ToUniversalTime()}");
				TestContext.WriteLine($"GenTime:{ats.ValidationData.TimeStampData.GenTime}");

				TestContext.WriteLine($"MessageImprintHashAlgorith:{ats.ValidationData.TimeStampData.HashAlgorithm}");
				TestContext.WriteLine($"SignerSignatureAlgorithm:{ats.ValidationData.TimeStampData.SignerSignatureAlgorithm}");
				TestContext.WriteLine($"SignerDigestAlgorithm:{ats.ValidationData.TimeStampData.SignerDigestAlgorithm}");
				TestContext.WriteLine($"CertificateDigestAlgorithm:{ats.ValidationData.TimeStampData.CertificateDigestAlgorithm}");

				//TestContext.WriteLine($"TargetValueLength:{ats.TargetValue.Length}");
				//TestContext.WriteLine($"TimeStampHash:{ats.TimeStampData.HashValue.ToHexString()}");
				//TestContext.WriteLine($"TargetValueHash:{ats.TimeStampData.CalculatedValue.ToHexString()}");

				//Assert
				Assert.Multiple(() =>
				{
					//証明書のエラーは出てしまう
					Assert.That(errors.Count == 1 && errors.Any(m => m.ItemName == "TSA証明書" && m.Message.StartsWith("証明書のパス検証が失敗しました。")));
					Assert.That(ats.ValidationData.TimeStampData.CalculatedValue, Is.EqualTo(ats.ValidationData.TimeStampData.HashValue));
				});
			}
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - (複数) - VALID")]
		public void VerifyMultiArchiveTimeStamp()
		{
			var config = new VerificationConfig();

			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.MultiATS);
			var data = new SignedDocumentXml(doc, "MultiATS.xml");

			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Unknown);
			TestData.SetESLevel(ESLevel.A, signature);

			foreach (var ats in signature.ArchiveTimeStampValidationData) {
				//次の世代のアーカイブタイムスタンプがあれば、その日付を使って検証する
				var nextGenAts = signature.ArchiveTimeStampValidationData.FirstOrDefault(m => m.Index == ats.Index + 1)?.ValidationData.TimeStamp;
				var validDate = nextGenAts?.ToTimeStampToken().TimeStampInfo.GenTime ?? DateTime.Now;

				ats.ValidationData.Validate(validDate, config, out TimeStampValidationExceptions errors);
				TestContext.WriteLine($"ATS Index:{ats.DispIndex}");
				TestContext.WriteLine($"Valid Date:{validDate.ToUniversalTime()}");
				TestContext.WriteLine($"GenTime:{ats.ValidationData.TimeStampData.GenTime}");

				TestContext.WriteLine($"SignerSignatureAlgorithm:{ats.ValidationData.TimeStampData.SignerSignatureAlgorithm}");
				TestContext.WriteLine($"SignerDigestAlgorithm:{ats.ValidationData.TimeStampData.SignerDigestAlgorithm}");
				TestContext.WriteLine($"CertificateDigestAlgorithm:{ats.ValidationData.TimeStampData.CertificateDigestAlgorithm}");

				//TestContext.WriteLine($"TargetValueLength:{ats.TargetValue.Length}");
				//TestContext.WriteLine($"TimeStampHash:{ats.TimeStampData.HashValue.ToHexString()}");
				//TestContext.WriteLine($"TargetValueHash:{ats.TimeStampData.CalculatedValue.ToHexString()}");

				//Assert
				{
					if (ats.DispIndex < 4) {
						Assert.That(errors, Has.Count.EqualTo(0));
					}
					else {
						//最後のATSはValidationDataが付いていない
						Assert.That(errors, Has.Count.EqualTo(1));
						Assert.Multiple(() =>
						{
							var item = errors.ElementAt(0);
							Assert.That(item.ItemName, Is.EqualTo("TSA証明書"));
							Assert.That(item.Message, Does.StartWith("トラストアンカーが見つかりません。"));
						});
					}

					Assert.That(ats.ValidationData.TimeStampData.CalculatedValue, Is.EqualTo(ats.ValidationData.TimeStampData.HashValue));
				}
			}
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - Reference(無し) - VALID")]
		public void VerifyHash_ReferenceError()
		{
			//SignedInfo配下のReference要素のみでよいのか？
			//Reference要素が一つも無い(一つもないと構造的にNGだがこの段階でもNGにすべきか...)
			var config = new VerificationConfig();
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundReference);
			var data = new SignedDocumentXml(doc, "NotFoundReference.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			var method = typeof(SignatureXml).GetMethod("AppendReferenceValue", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(signature, new object[] { totalValue });
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - SignedInfo(無し) - INVALID")]
		public void VerifyHash_SignedInfoNoneError()
		{
			var config = new VerificationConfig();
			//SignedInfo要素が無い
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundSignedInfo);
			var data = new SignedDocumentXml(doc, "NotFoundSignedInfo.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			//リフレクションなので一つくるまれる
			var ex = Assert.Throws<TargetInvocationException>(() =>
			{
				var method = typeof(SignatureXml).GetMethod("AppendSignedInfoValue", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(signature, new object[] { null, totalValue });
			});

			Assert.Multiple(() =>
			{
				Assert.That(ex, Is.Not.Null);

				var tsException = ex.InnerException as TimeStampConvertException;

				Assert.That(tsException, Is.Not.Null);
				Assert.That(tsException.GetType(), Is.EqualTo(typeof(TimeStampConvertException)));
				Assert.That(tsException.ItemName, Is.EqualTo(TimeStampItemNames.Target));
				Assert.That(tsException.Message, Is.EqualTo("SignedInfo要素の取得(正規化)に失敗しました。"));
			});
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - SignatureValue(無し) - INVALID")]
		public void VerifyHash_SignatureValueNoneError()
		{
			var config = new VerificationConfig();

			//SignatureValue要素が無い
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundSignatureValue);
			var data = new SignedDocumentXml(doc, "NotFoundSignatureValue.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			//リフレクションなので一つくるまれる
			var ex = Assert.Throws<TargetInvocationException>(() =>
			{
				var method = typeof(SignatureXml).GetMethod("AppendSignatureValue", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(signature, new object[] { null, totalValue });
			});

			Assert.Multiple(() =>
			{
				Assert.That(ex, Is.Not.Null);

				var tsException = ex.InnerException as TimeStampConvertException;

				Assert.That(tsException, Is.Not.Null);
				Assert.That(tsException.GetType(), Is.EqualTo(typeof(TimeStampConvertException)));
				Assert.That(tsException.ItemName, Is.EqualTo(TimeStampItemNames.Target));
				Assert.That(tsException.Message, Is.EqualTo("SignatureValue要素の取得(正規化)に失敗しました。"));
			});
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - KeyInfo(無し) - VALID")]
		public void VerifyHash_KeyInfoNone()
		{
			var config = new VerificationConfig();
			//KeyInfo要素が無い
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundKeyInfo);
			var data = new SignedDocumentXml(doc, "NotFoundKeyInfo.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			var method = typeof(SignatureXml).GetMethod("AppendKeyInfoValue", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(signature, new object[] { null, totalValue });
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - UnsignedSignatureProperties(無し) - INVALID")]
		public void VerifyHash_UnsignedSignaturePropsNoneError()
		{
			//CertificateValuesとRevocationValuesは必須とあるが、
			//個別にエラーを出す必要があるか？構造検証側で出るからいいかな。
			var config = new VerificationConfig();
			//UnsignedSignatureProperties要素が無い
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundUnsignedSignatureProperties);
			var data = new SignedDocumentXml(doc, "NotFoundUnsignedSignatureProperties.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			//リフレクションなので一つくるまれる
			var ex = Assert.Throws<TargetInvocationException>(() =>
			{
				var method = typeof(SignatureXml).GetMethod("AppendUnsignedSignaturePropValue", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(signature, new object[] { null, "ATS", totalValue });
			});

			Assert.Multiple(() =>
			{
				Assert.That(ex, Is.Not.Null);

				var tsException = ex.InnerException as TimeStampConvertException;

				Assert.That(tsException, Is.Not.Null);
				Assert.That(tsException.GetType(), Is.EqualTo(typeof(TimeStampConvertException)));
				Assert.That(tsException.ItemName, Is.EqualTo(TimeStampItemNames.Target));
				Assert.That(tsException.Message, Is.EqualTo("非署名属性要素の取得(正規化)に失敗しました。"));
			});
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプ対象データ - Object(無し) - VALID")]
		public void VerifyHash_ObjectsNone()
		{
			var config = new VerificationConfig();
			//Object要素が無い(一つもないと構造的にNGだがこの段階ではOK)
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundObject);
			var data = new SignedDocumentXml(doc, "NotFoundObject.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var totalValue = new List<byte>();

			var method = typeof(SignatureXml).GetMethod("AppendObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(signature, new object[] { null, totalValue });
		}
	}
}
