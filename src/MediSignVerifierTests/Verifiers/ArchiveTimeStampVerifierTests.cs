using System;
using System.Linq;
using NUnit.Framework;
using SignatureVerifier.Properties;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Verifiers
{
	public class ArchiveTimeStampVerifierTests
	{
		[Test(Description = "(調剤)5.6.3 アーカイブタイムスタンプの検証要件 - MessageImprint値 - VALID")]
		public void VerifyDispensingArchiveTimeStampFromXml()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing-005_03.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ArchiveTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("アーカイブタイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				//ハッシュ値はOK
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 1 && m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - アーカイブタイムスタンプ要素 - 存在チェック - INVALID")]
		public void VerifyStructure_TimeStampExistsError()
		{
			//この要素がなければ、そもそも-Aにはならないはずなんだけど
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.NotFoundATS);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "NotFoundATS.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
			TestData.SetESLevel(ESLevel.A, signature);

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ArchiveTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("アーカイブタイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.ItemName == "")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.ItemName == "")?.Message, Is.EqualTo("アーカイブタイムスタンプが見つかりませんでした。"));
			});
		}

		[Test(Description = "5.6.3 アーカイブタイムスタンプの検証要件 - タイムスタンプトークン(複数：2番目改ざん) - INVALID")]
		public void VerifyMultiTimeStampError()
		{
			var doc = TestData.CreateXmlDocument(Resources_ArchiveTimeStamp.MultiATSBase64Error);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "MultiATSBase64Error.xml");
			var signature = data.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.None);
			TestData.SetESLevel(ESLevel.A, signature);

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ArchiveTimeStampVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.Now);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("アーカイブタイムスタンプ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 1 && m.ItemName == "検証基準時刻")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 1 && m.ItemName == "検証基準時刻")?.Message, Is.EqualTo("次世代アーカイブタイムスタンプ生成時刻の取得に失敗しました。"));

				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 2 && m.ItemName == "タイムスタンプトークン")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 2 && m.ItemName == "タイムスタンプトークン")?.Message, Is.EqualTo("タイムスタンプのデコードに失敗しました。"));

				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 3 && m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 3 && m.ItemName == "MessageImprint値")?.Message, Is.EqualTo("計算したハッシュ値とhashMessageの値が一致しません。"));

				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 4 && m.ItemName == "MessageImprint値")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "アーカイブタイムスタンプ" && m.Index == 4 && m.ItemName == "MessageImprint値")?.Message, Is.EqualTo("計算したハッシュ値とhashMessageの値が一致しません。"));
			});
		}
	}
}
