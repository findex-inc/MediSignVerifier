using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;

namespace SignatureVerifier.Verifiers
{
	public partial class SigningCertificateVerifierTests
	{
		private readonly Fixtures _fixtures = new Fixtures();

		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - VALID")]
		public void Verify([Values] ESLevel level)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestCertificatePathValidationData(utcDateTime);

			var config = new VerificationConfig();

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.SetupGet(x => x.IssuerSerial).Returns((CertificateIssuerSerialData)null);
			validationData.SetupGet(x => x.CertDigest).Returns((CertificateDigestData)null);
			validationData.SetupGet(x => x.Certificate).Returns(eeCert);
			validationData.SetupGet(x => x.PathValidationData).Returns(pathData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(level);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			if (level == ESLevel.A) {
				signature.SetupGet(x => x.SignatureTimeStampGenTime).Returns(utcDateTime);
			}

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			void handler(object s, VerifiedEventArgs e)
			{
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object,
				(level == ESLevel.A) ? DateTime.MaxValue : utcDateTime.ToLocalTime());

			target.VerifiedEvent -= handler;

			//Assert
			validationData.Verify();
			signature.Verify();
			doc.Verify();

			if (level == ESLevel.None) {

				Assert.Multiple(() =>
				{
					Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(actual.Items.Count(), Is.EqualTo(0));
				});

			}
			else {

				Assert.Multiple(() =>
				{
					Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
					Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(actual.Items.Count(), Is.EqualTo(5));
				});

				Assert.Multiple(() =>
				{
					var item = actual.Items.ElementAt(0);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名者証明書"));
					Assert.That(item.ItemName, Is.EqualTo("証明書の指定確認"));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = actual.Items.ElementAt(1);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名者証明書"));
					Assert.That(item.ItemName, Is.EqualTo("証明書の実体確認"));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = actual.Items.ElementAt(2);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名者証明書"));
					Assert.That(item.ItemName, Is.EqualTo("証明書の一致確認"));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = actual.Items.ElementAt(3);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名者証明書"));
					Assert.That(item.ItemName, Is.EqualTo("HPKI固有項目の確認"));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.Null);
				});

				Assert.Multiple(() =>
				{
					var item = actual.Items.ElementAt(4);

					Assert.That(item.Status, Is.EqualTo(VerificationStatus.VALID));
					Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
					Assert.That(item.Source, Is.EqualTo("署名者証明書"));
					Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
					Assert.That(item.MappedItem, Is.Null);
					Assert.That(item.Message, Is.Null);
				});
			}

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(0));
			});

		}


		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - 署名者証明書の指定確認 - INVALID")]
		public void Verify_WithCertificateLocation_IsInvalid()
		{
			//Arrange
			var config = new VerificationConfig();

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.BES);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.None);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns((ISigningCertificateValidationData)null);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			signature.Verify();
			doc.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count(), Is.EqualTo(1));
			});

			Assert.Multiple(() =>
			{
				var item = actual.Items.ElementAt(0);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.None));
				Assert.That(item.Source, Is.EqualTo("署名者証明書"));
				Assert.That(item.ItemName, Is.EqualTo("証明書の指定確認"));
				Assert.That(item.MappedItem, Is.Null);
				Assert.That(item.Message, Is.EqualTo("署名者証明書が見つかりません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(1));

				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名者証明書："));
				Assert.That(actualEvent.Message, Is.EqualTo("署名者証明書が見つかりません。"));
			});
		}

		public enum InvalidDecodedType
		{
			IsNull,
			IsNullValue,
			IlligalValue,
		}

		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - 署名者証明書の実体確認 - INVALID")]
		public void Verify_WithCertificateDecoded_IsInvalid([Values] InvalidDecodedType type)
		{
			//Arrange
			var config = new VerificationConfig();

			CertificateData cert = null;
			switch (type) {
				case InvalidDecodedType.IsNull:
					break;
				case InvalidDecodedType.IsNullValue:
					cert = new CertificateData("null value ", null);
					break;
				case InvalidDecodedType.IlligalValue:
					cert = new CertificateData("illigal-value", new byte[] { 0x00, 0x00, 0x00, 0x00 });
					break;
				default:
					throw new InvalidOperationException($"Invalid test type.");
			}

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.Setup(x => x.Certificate).Returns(cert);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.BES);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			validationData.Verify();
			signature.Verify();
			doc.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count(), Is.EqualTo(2));
			});

			Assert.Multiple(() =>
			{
				var item = actual.Items.ElementAt(1);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Prescription));
				Assert.That(item.Source, Is.EqualTo("署名者証明書"));
				Assert.That(item.ItemName, Is.EqualTo("証明書の実体確認"));
				Assert.That(item.MappedItem, Is.EqualTo(null));
				Assert.That(item.Message, Is.EqualTo("署名者証明書の実体が確認できません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(1));

				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名者証明書："));
				Assert.That(actualEvent.Message, Is.EqualTo("署名者証明書の実体が確認できません。"));
			});

		}

		public enum InvalidConsistencyType
		{
			SigningCertificateIssuerSerial,
			SigningCertificateDigestValue,
			KeyInfo_X509IssuerSerial
		}


		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - 署名者証明書の一致確認 - INVALID")]
		public void Verify_WithCertificateConsistency_IsInvalid([Values] InvalidConsistencyType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, _, trust) = _fixtures.GetTestCertificatePathValidationData(utcDateTime);

			var config = new VerificationConfig();

			// 参照と実態が一致しなかったら、CertificateValuesから実態を確認できないので「証明書の実体確認」で先にエラーになる?
			// Issuer一致でSerial違う場合か？？

			var validationData = new Mock<ISigningCertificateValidationData>();
			switch (type) {
				case InvalidConsistencyType.SigningCertificateIssuerSerial: {
						var isssurSerial = new CertificateIssuerSerialData("cert-id", "CN=Test Intermediate CA Certificate", "122");
						validationData.SetupGet(x => x.IssuerSerial).Returns(isssurSerial);
						validationData.SetupGet(x => x.Certificate).Returns(eeCert);
					}
					break;

				case InvalidConsistencyType.SigningCertificateDigestValue: {
						var isssurSerial = new CertificateIssuerSerialData("cert-id", "CN=Test Intermediate CA Certificate", "12345");
						var attribute = "http://www.w3.org/2001/04/xmlenc#sha512";
						var digestValue = new CertificateDigestData("cert-id", attribute, new byte[] { 0x00, 0x01, 0x04, 0x04 });
						validationData.SetupGet(x => x.IssuerSerial).Returns(isssurSerial);
						validationData.SetupGet(x => x.CertDigest).Returns(digestValue);
						validationData.SetupGet(x => x.Certificate).Returns(eeCert);
					}
					break;

				case InvalidConsistencyType.KeyInfo_X509IssuerSerial: {
						var isssurSerial = new CertificateIssuerSerialData("keyinfo-id", "CN=Test Certificate", "998");
						validationData.SetupGet(x => x.IssuerSerial).Returns(isssurSerial);
						validationData.SetupGet(x => x.Certificate).Returns(eeCert);
					}
					break;

				default:
					throw new InvalidOperationException($"Invalid test type.");
			}

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.BES);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			validationData.Verify();
			signature.Verify();
			doc.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count(), Is.EqualTo(3));
			});

			Assert.Multiple(() =>
			{
				var item = actual.Items.ElementAt(2);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Dispensing));
				Assert.That(item.Source, Is.EqualTo("署名者証明書"));
				Assert.That(item.ItemName, Is.EqualTo("証明書の一致確認"));
				Assert.That(item.MappedItem, Is.EqualTo("#signning"));
				Assert.That(item.Message, Does.Match(@"^署名者証明書の参照と実体が一致しません。Issuer=.+?, SerialNumber=.+?, Certificate=.+"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(1));

				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名者証明書：#signning"));
				Assert.That(actualEvent.Message, Does.Match(@"^署名者証明書の参照と実体が一致しません。Issuer=.+?, SerialNumber=.+?, Certificate=.+"));
			});
		}


		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - 署名者証明書のパス構築とパス検証 - INVALID")]
		public void Verify_WithCertificatePath_IsInvalid()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestCertificatePathValidationData(utcDateTime);

			var config = new VerificationConfig();

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.SetupGet(x => x.IssuerSerial).Returns((CertificateIssuerSerialData)null);
			validationData.SetupGet(x => x.CertDigest).Returns((CertificateDigestData)null);
			validationData.SetupGet(x => x.Certificate).Returns(eeCert);
			validationData.SetupGet(x => x.PathValidationData).Returns(pathData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.BES);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			validationData.Verify();
			signature.Verify();
			doc.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Items.Count(), Is.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				var item = actual.Items.ElementAt(4);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Dispensing));
				Assert.That(item.Source, Is.EqualTo("署名者証明書"));
				Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
				Assert.That(item.MappedItem, Is.EqualTo("#signning"));
				Assert.That(item.Message, Does.Match(@"^証明書が有効期間内にありません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(1));

				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actualEvent.Source, Is.EqualTo("署名者証明書：#signning"));
				Assert.That(actualEvent.Message, Does.Match("^証明書が有効期間内にありません。"));
			});
		}


		[Test(Description = "5.5.3 XAdES の検証要件 - 署名者証明書 - 署名者証明書のパス構築とパス検証 - INDETERMINATE")]
		public void Verify_WithCertificatePathIsNull_IsIndeterminate()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, _, trust) = _fixtures.GetTestCertificatePathValidationData(utcDateTime);
			var pathData = new CertificatePathValidationData("unknown", new[] { trust.Value }, null);

			var config = new VerificationConfig();

			var validationData = new Mock<ISigningCertificateValidationData>();
			validationData.SetupGet(x => x.IssuerSerial).Returns((CertificateIssuerSerialData)null);
			validationData.SetupGet(x => x.CertDigest).Returns((CertificateDigestData)null);
			validationData.SetupGet(x => x.Certificate).Returns(eeCert);
			validationData.SetupGet(x => x.PathValidationData).Returns(pathData);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.ESLevel).Returns(ESLevel.BES);
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			signature.SetupGet(x => x.SigningCertificateValidationData).Returns(validationData.Object);

			var doc = new Mock<ISignedDocument>();
			doc.SetupGet(x => x.Signatures).Returns(new[] { signature.Object });

			int eventCounter = 0;
			VerifiedEventArgs actualEvent = null;
			void handler(object s, VerifiedEventArgs e)
			{
				actualEvent = e;
				eventCounter++;
			}

			IVerifier target = new SigningCertificateVerifier(config);
			target.VerifiedEvent += handler;

			//Act.
			var actual = target.Verify(doc.Object, DateTime.MaxValue);

			target.VerifiedEvent -= handler;

			//Assert
			validationData.Verify();
			signature.Verify();
			doc.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Source, Is.EqualTo("署名者証明書"));
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(actual.Items.Count(), Is.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				var item = actual.Items.ElementAt(4);

				Assert.That(item.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(item.Type, Is.EqualTo(SignatureSourceType.Dispensing));
				Assert.That(item.Source, Is.EqualTo("署名者証明書"));
				Assert.That(item.ItemName, Is.EqualTo("証明書のパス構築とパス検証"));
				Assert.That(item.MappedItem, Is.EqualTo("#signning"));
				Assert.That(item.Message, Is.EqualTo("上位証明書が見つかりません。"));
			});

			Assert.Multiple(() =>
			{
				Assert.That(eventCounter, Is.EqualTo(1));

				Assert.That(actualEvent.Status, Is.EqualTo(VerificationStatus.INDETERMINATE));
				Assert.That(actualEvent.Source, Is.EqualTo("署名者証明書：#signning"));
				Assert.That(actualEvent.Message, Does.Match("^上位証明書が見つかりません。"));
			});
		}


	}
}
