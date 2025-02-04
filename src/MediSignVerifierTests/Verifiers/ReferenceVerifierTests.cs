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
	[TestFixture]
	internal class ReferenceVerifierTests
	{
		[Test(Description = "（処方）5.5.3 XAdESの検証要件 - 参照データ - VALID")]
		public void VerifyPrescriptionReferences()
		{
			var doc = TestData.CreateXmlDocument(Resources.Prescription_004_01);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Prescription-004_01.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.Where(m => m.ItemName == "Reference要素").Count(), Is.EqualTo(3));
				Assert.That(result.Items.Where(m => m.ItemName == "Reference要素" && m.Status == VerificationStatus.VALID).Count(), Is.EqualTo(3));
				//Assert.That(result.Items.Where(m => m.ItemName == "DigestMethod要素").Count(), Is.EqualTo(3));
			});
		}

		[Test(Description = "（調剤）5.5.3 XAdESの検証要件 - 参照データ - VALID")]
		public void VerifyDispensingReferences()
		{
			var doc = TestData.CreateXmlDocument(Resources.Dispensing_005_03);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "Dispensing-005_03.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.Where(m => m.ItemName == "Reference要素").Count(), Is.EqualTo(6));
				Assert.That(result.Items.Where(m => m.ItemName == "Reference要素" && m.Status == VerificationStatus.VALID).Count(), Is.EqualTo(6));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "iddb33cd61-Reference1-Detached").Status, Is.EqualTo(VerificationStatus.VALID));

				//Assert.That(result.Items.Where(m => m.ItemName == "DigestMethod要素").Count(), Is.EqualTo(6));
			});
		}

		[Test(Description = "（処方）5.5.3 XAdESの検証要件 - 参照データ - INVALID")]
		public void VerifyPrescriptionReference_Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_Reference.PrescriptionReferenceError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "PrescriptionReferenceError.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Index == 1 && m.ItemName == "Reference要素" && m.MappedItem == "id29b12603-Reference1-Detached").Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(result.Items.FirstOrDefault(m => m.Index == 2 && m.ItemName == "Reference要素" && m.MappedItem == "id041cb21b-Reference2-KeyInfo").Message, Is.EqualTo("ハッシュ値の計算に失敗しました。"));
				Assert.That(result.Items.FirstOrDefault(m => m.Index == 3 && m.ItemName == "Reference要素" && m.MappedItem == "id8b2ae0fe-Reference3-SignedProperties").Message, Is.EqualTo("DigestValueのデコードに失敗しました。"));

				Assert.That(result.Items.FirstOrDefault(m => m.Index == 2 && m.ItemName == "DigestMethod要素" && m.MappedItem == "id041cb21b-Reference2-KeyInfo").Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}

		[Test(Description = "（調剤）5.5.3 XAdESの検証要件 - 参照データ - INVALID")]
		public void VerifyDispensingReference_Error()
		{
			var doc = TestData.CreateXmlDocument(Resources_Reference.DispensingReferenceError);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, "DispensingReferenceError.xml");

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "id041cb21b-Reference2-KeyInfo").Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "id5813f70a-Reference3-SignedProperties").Message, Is.EqualTo("計算したハッシュ値とDigestValueの値が一致しません。"));

				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "id29b12603-Reference1-Detached").Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "id8b2ae0fe-Reference3-SignedProperties").Status, Is.EqualTo(VerificationStatus.VALID));

				//Dispensing配下をいじったので、こちらのハッシュも狂う
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "iddb33cd61-Reference1-Detached").Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "Reference要素" && m.MappedItem == "idfbb8185d-Reference2-KeyInfo").Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "5.5.3 XAdESの検証要件 - 参照データ - Reference要素 - INVALID")]
		public void VerifyReferences_DigestValue_Error()
		{
			var config = new VerificationConfig();

			var refList = new List<ReferenceValidationData>
			{
				new ReferenceValidationData("1", 1, "#ref-1", "", "", System.Text.Encoding.ASCII.GetBytes("test"), new byte[0])
			};

			var signature = new Mock<ISignature>();
			signature.SetupGet<IEnumerable<ReferenceValidationData>>(m => m.ReferenceValidationData).Returns(refList.ToArray());
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.BES);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet<IEnumerable<ISignature>>(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
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
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.Where(m => m.ItemName == "Reference要素").Count(), Is.EqualTo(1));
			});
		}

		[Test(Description = "5.5.3 XAdESの検証要件 - 参照データ - DigestMethod要素 - INVALID")]
		public void VerifyReferences_DigestMethod_Error()
		{
			var config = new VerificationConfig();

			var refList = new List<ReferenceValidationData>
			{
				new ReferenceValidationData("1", 1, "#ref-1", "", "", new byte[0], new byte[0]),
				new ReferenceValidationData("2", 2, "#ref-2", "", "http://www.w3.org/2001/10/xml-exc-c14n#", new byte[0], new byte[0]),
				new ReferenceValidationData("3", 3, "#ref-3", "", "http://www.w3.org/2001/04/xmlenc#sha256", new byte[0], new byte[0])
			};

			var signature = new Mock<ISignature>();
			signature.SetupGet<IEnumerable<ReferenceValidationData>>(m => m.ReferenceValidationData).Returns(refList.ToArray());
			signature.SetupGet(m => m.ESLevel).Returns(ESLevel.BES);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet<IEnumerable<ISignature>>(m => m.Signatures).Returns(new[] { signature.Object });

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IVerifier target = new ReferenceVerifier(config);
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
				Assert.That(result.Source, Is.EqualTo("参照データ"));
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "DigestMethod要素" && m.MappedItem == "1").Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "DigestMethod要素" && m.MappedItem == "2").Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.ItemName == "DigestMethod要素" && m.MappedItem == "3").Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}
	}
}
