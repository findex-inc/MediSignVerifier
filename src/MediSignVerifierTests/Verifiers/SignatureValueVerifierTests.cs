using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Properties;
using Resources = MediSignVerifier.Tests.Properties.Resources;

namespace SignatureVerifier.Verifiers
{
	internal class SignatureValueVerifierTests
	{
		[Test(Description = "（処方）5.5.3 XAdESの検証要件 - 署名データ - VALID")]
		public void VerifyPrescriptionSignature()
		{
			var doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription-004_01.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureValueVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "（調剤）5.5.3 XAdESの検証要件 - 署名データ - VALID")]
		public void VerifyDispensingSignature()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing-005_03.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureValueVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Dispensing && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Dispensing && m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.DispPrescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.DispPrescription && m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		//Base64に失敗
		[Test(Description = "5.5.3 XAdESの検証要件 - 署名データ - INVALID(Base64エラー)")]
		public void VerifySignature_Base64Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureValue.SignatureBase64Error);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "SignatureBase64Error.xml");

			IVerifier target = new SignatureValueVerifier(config);

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			Assert.Multiple(() =>
			{
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Message, Is.EqualTo("署名値のデコードに失敗しました。"));
			});
		}

		//署名者証明書の改ざん
		[Test(Description = "5.5.3 XAdESの検証要件 - 署名データ - INVALID(証明書改ざんエラー)")]
		public void VerifySignature_KeyInfoError()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureValue.KeyInfoError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "KeyInfoError.xml");

			IVerifier target = new SignatureValueVerifier(config);

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			Assert.Multiple(() =>
			{
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Message, Is.EqualTo("署名値の復号に失敗しました。"));
			});
		}

		//SignedInfoの正規化
		[Test(Description = "5.5.3 XAdESの検証要件 - 署名データ - INVALID(正規化エラー)")]
		public void VerifySignature_SignedInfoError()
		{
			var doc = TestData.CreateXmlDocument(Resources_SignatureValue.SignedInfoError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "SignedInfoError.xml");

			IVerifier target = new SignatureValueVerifier(config);

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			Assert.Multiple(() =>
			{
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Message, Is.EqualTo("正規化アルゴリズムが不正です。"));
			});
		}

		//署名値が一致しない
		[Test(Description = "（処方）5.5.3 XAdESの検証要件 - 署名データ - INVALID")]
		public void VerifyPrescriptionSignature_Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_Reference.PrescriptionReferenceError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "PrescriptionReferenceError.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureValueVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "SignatureValue要素" && m.MappedItem == "id00fac23c-SignatureValue").Message, Is.EqualTo("復号した署名値と計算した署名値が一致しません。"));
			});
		}

		[Test(Description = "5.5.3 XAdESの検証要件 - 署名データ - SignatureMethod要素 - VALID")]
		[TestCase("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")]
		[TestCase("http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256")]
		public void VerifySignature_SignatureMethod(string algorithm)
		{
			var config = new VerificationConfig();

			var data = new SignatureValueValidationData("1", "http://www.w3.org/2001/10/xml-exc-c14n#", algorithm, new byte[0], new byte[0]);

			var signature = new Mock<ISignature>();
			signature.SetupGet<SignatureValueValidationData>(m => m.SignatureValueValidationData).Returns(data);
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.BES);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet<IEnumerable<ISignature>>(m => m.Signatures).Returns(new[] { signature.Object });

			IVerifier target = new SignatureValueVerifier(config);

			//Act.
			var result = target.Verify(doc.Object, DateTime.MaxValue);

			doc.Verify();
			signature.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "SignatureMethod要素" && m.MappedItem == "1").Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "5.5.3 XAdESの検証要件 - 署名データ - SignatureMethod要素 - INVALID")]
		[TestCase("")]
		[TestCase(null)]
		[TestCase("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256")]
		public void VerifySignature_SignatureMethod_Error(string algorithm)
		{
			var config = new VerificationConfig();

			var data = new SignatureValueValidationData("1", "http://www.w3.org/2001/10/xml-exc-c14n#", algorithm, new byte[0], new byte[0]);

			var signature = new Mock<ISignature>();
			signature.SetupGet<SignatureValueValidationData>(m => m.SignatureValueValidationData).Returns(data);
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.BES);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet<IEnumerable<ISignature>>(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new SignatureValueVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			//TODO正しくmockをつかったかどうかの判定
			doc.Verify();
			signature.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("署名データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "SignatureMethod要素" && m.MappedItem == "1").Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "SignatureMethod要素" && m.MappedItem == "1").Message, Is.EqualTo("サポートされていない署名アルゴリズムが指定されています。"));
			});
		}
	}
}
