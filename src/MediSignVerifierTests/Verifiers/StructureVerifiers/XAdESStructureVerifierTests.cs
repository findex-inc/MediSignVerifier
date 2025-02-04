using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Resources = SignatureVerifier.Properties.Resources_XAdESStructureVerifierTests;

namespace SignatureVerifier.Verifiers.StructureVerifiers
{
	internal class XAdESStructureVerifierTests

	{
		[Test(Description = "署名構造 - XAdES必須要素 - VALID")]
		public void Verify()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.FoundRequiredTag);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, null);

			var isValid = true;

			void handler(object s, VerifiedEventArgs e)
			{
				isValid = false;
			}

			IStructureVerifier target = new XAdESStructureVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);

				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(result.Source, Is.EqualTo("署名構造"));
				Assert.That(result.Items.Count(), Is.EqualTo(2));
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(item.Source, Is.EqualTo("署名構造"));
				Assert.That(item.ItemName, Is.EqualTo("XAdES必須要素"));
				Assert.That(item.MappedItem, Is.Null);
				Assert.That(item.Message, Is.Null);
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(item.Source, Is.EqualTo("署名構造"));
				Assert.That(item.ItemName, Is.EqualTo("SignedProperties要素"));
				Assert.That(item.MappedItem, Is.Null);
				Assert.That(item.Message, Is.Null);
			});
		}


		[Test(Description = "署名構造 - XAdES必須要素 - Sigunature[@Id] - INVALID")]
		public void VerifyMandatoryNodes_WithErrorUndefinedSigunatureId()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.UndefinedSigunatureId);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, null);

			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
			}

			IStructureVerifier target = new XAdESStructureVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Source, Is.EqualTo("署名構造"));
				Assert.That(result.Items.Count(), Is.EqualTo(2));
			});

			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(item.Source, Is.EqualTo("署名構造"));
				Assert.That(item.ItemName, Is.EqualTo("XAdES必須要素"));
				Assert.That(item.MappedItem, Is.Null);
				Assert.That(item.Message, Is.EqualTo("Signature要素のId属性が見つかりません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名構造："));
				Assert.That(actualEvent.Message, Is.EqualTo("Signature要素のId属性が見つかりません。"));
			});

		}

		[Test(Description = "署名構造 - XAdES必須要素 - XAdES-BES - INVALID")]
		public void VerifyMandatoryNodes_WithErrorBES()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.NotFoundRequiredTagBES);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, null);

			var actualEvents = new List<VerifiedEventArgs>();
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvents.Add(e);
			}

			IStructureVerifier target = new XAdESStructureVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Source, Is.EqualTo("署名構造"));
				Assert.That(result.Items.Count(), Is.EqualTo(7));
			});

			var expecteds = new[]
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:SignedInfo/xs:CanonicalizationMethod\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:SignedInfo/xs:SignatureMethod\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:SignedInfo/xs:Reference\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:SignedInfo/xs:Reference/xs:DigestMethod\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:SignedInfo/xs:Reference/xs:DigestValue\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:Object/xa:QualifyingProperties/xa:SignedProperties/xa:SignedSignatureProperties\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, null, "SignedProperties要素", null,
					null),
			};

			foreach (var i in Enumerable.Range(0, expecteds.Length)) {

				var expected = expecteds[i];

				Assert.Multiple(() =>
				{
					var item = result.Items.ElementAt(i);

					Assert.That(item.Status, Is.EqualTo(expected.Status));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名構造"));
					Assert.That(item.ItemName, Is.EqualTo(expected.ItemName));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.EqualTo(expected.Message));
				});

				if (expected.Status != VerificationStatus.VALID) {

					var actualEvent = actualEvents.ElementAt(i);

					Assert.Multiple(() =>
					{
						Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
						Assert.That(actualEvent.Source, Is.EqualTo("署名構造："));
						Assert.That(actualEvent.Message, Is.EqualTo(expected.Message));
					});
				}
			}

		}

		[Test(Description = "署名構造 - XAdES必須要素 - XAdES-A - INVALID")]
		public void VerifyMandatoryNodes_WithErrorA()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.NotFoundRequiredTagA);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, null);

			var actualEvents = new List<VerifiedEventArgs>();
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvents.Add(e);
			}

			IStructureVerifier target = new XAdESStructureVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Source, Is.EqualTo("署名構造"));
				Assert.That(result.Items.Count(), Is.EqualTo(2));
			});

			var expecteds = new[]
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, null, "XAdES必須要素", null,
					"\"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/xa:RevocationValues\"要素が見つかりません。"),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, null, "SignedProperties要素", null,
					null),
			};

			foreach (var i in Enumerable.Range(0, expecteds.Length)) {

				var expected = expecteds[i];

				Assert.Multiple(() =>
				{
					var item = result.Items.ElementAt(i);

					Assert.That(item.Status, Is.EqualTo(expected.Status));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名構造"));
					Assert.That(item.ItemName, Is.EqualTo(expected.ItemName));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.EqualTo(expected.Message));
				});

				if (expected.Status != VerificationStatus.VALID) {

					var actualEvent = actualEvents.ElementAt(i);

					Assert.Multiple(() =>
					{
						Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
						Assert.That(actualEvent.Source, Is.EqualTo("署名構造："));
						Assert.That(actualEvent.Message, Is.EqualTo(expected.Message));
					});
				}
			}

		}

		[Test(Description = "署名構造 - SignedProperties要素 - INVALID")]
		public void VerifySignedPropertiesNode_WithErrorType()
		{
			// Arrange.
			var doc = new XmlDocument();
			doc.LoadXml(Resources.NotFoundSignedPropertiesType);

			var config = new VerificationConfig();
			var data = new SignedDocumentXml(doc, null);

			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
			}

			IStructureVerifier target = new XAdESStructureVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Source, Is.EqualTo("署名構造"));
				Assert.That(result.Items.Count(), Is.EqualTo(2));
			});


			Assert.Multiple(() =>
			{
				var item = result.Items.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(item.Source, Is.EqualTo("署名構造"));
				Assert.That(item.ItemName, Is.EqualTo("SignedProperties要素"));
				Assert.That(item.MappedItem, Is.Null);
				Assert.That(item.Message, Is.EqualTo("正しいType属性がセットされていません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名構造："));
				Assert.That(actualEvent.Message, Is.EqualTo("正しいType属性がセットされていません。"));
			});

		}
	}
}

