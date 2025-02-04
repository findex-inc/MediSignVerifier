using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace SignatureVerifier
{
	public class SignatureVerifierTests
	{

		[Test(Description = "構造検証のみで検証し正常終了すること。")]
		public void Verify_WithEmptyVerifiers()
		{
			//Arrange
			var config = new VerificationConfig();

			var structureVerifier = new Mock<IStructureVerifier>();
			var structureVerifierResult = new VerificationResult(VerificationStatus.VALID, null);
			structureVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>())).Returns(structureVerifierResult);

			var doc = new Mock<ISignedDocument>();
			doc.Setup(x => x.CreateStructureVerifier(config)).Returns(structureVerifier.Object);

			var eventCalledCount = 0;

			var verifier = new SignatureVerifier(config);
			verifier.Clear();
			verifier.VerifiedEvent += (s, e) =>
			{
				eventCalledCount++;
			};

			//Act.
			var actual = verifier.Verify(doc.Object, DateTime.MaxValue);

			//Assert
			doc.Verify();
			structureVerifier.Verify();
			Assert.Multiple(() =>
			{
				//署名が一つも無い場合はINVALID
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.VerificationResults.Count(), Is.EqualTo(1));
			});
			var actualResult = actual.VerificationResults.First();
			Assert.Multiple(() =>
			{
				Assert.That(actualResult.Status, Is.EqualTo(VerificationStatus.VALID));

				Assert.That(eventCalledCount, Is.EqualTo(0), "event is called.");
			});
			return;
		}

		[Test(Description = "構造検証でINVALIDの場合には以降の検証を行わないこと。")]
		public void Verify_WithStructureVerifiierIsInvalid_SkipAfter()
		{
			//Arrange
			var config = new VerificationConfig();

			var structureVerifier = new Mock<IStructureVerifier>();
			var structureVerifierResult = new VerificationResult(VerificationStatus.INVALID, null);
			var structureVerifierEventArgs = new VerifiedEventArgs(VerificationStatus.INVALID, "構造検証", "エラー");
			structureVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>()))
				.Callback(() => structureVerifier.Raise(x => x.VerifiedEvent += null, structureVerifierEventArgs))
				.Returns(structureVerifierResult);

			var doc = new Mock<ISignedDocument>();
			doc.Setup(x => x.CreateStructureVerifier(config)).Returns(structureVerifier.Object);

			var eventCalledCount = 0;

			var verifier = new SignatureVerifier(config);
			//verifier.Clear();
			verifier.VerifiedEvent += (s, e) =>
			{
				eventCalledCount++;
			};

			//Act.
			var actual = verifier.Verify(doc.Object, DateTime.MaxValue);

			//Assert
			doc.Verify();
			structureVerifier.Verify();

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.VerificationResults.Count(), Is.EqualTo(1));
			});

			var actualResult = actual.VerificationResults.First();

			Assert.Multiple(() =>
			{
				Assert.That(actualResult.Status, Is.EqualTo(VerificationStatus.INVALID));
			});

			Assert.That(eventCalledCount, Is.EqualTo(1), "event is called.");

			return;
		}

		[Test(Description = "検証を順次実行すること。")]
		public void Verify_WithAnyVerifier()
		{
			//Arrange
			var config = new VerificationConfig();

			var structureVerifier = new Mock<IStructureVerifier>();
			var structureVerifierResult = new VerificationResult(VerificationStatus.VALID, null);
			structureVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>())).Returns(structureVerifierResult);

			var doc = new Mock<ISignedDocument>();
			doc.Setup(x => x.CreateStructureVerifier(config)).Returns(structureVerifier.Object);

			var mockVerifiers = new List<Mock<IVerifier>>();
			foreach (var i in Enumerable.Range(0, 5)) {

				var mockVerifier = new Mock<IVerifier>();
				var mockResult = new VerificationResult(VerificationStatus.VALID, null);
				mockVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>(), It.IsAny<DateTime>()))
					.Returns(mockResult);

				mockVerifiers.Add(mockVerifier);
			}

			var eventCalledCount = 0;

			var verifier = new SignatureVerifier(config);
			verifier.Clear();
			foreach (var v in mockVerifiers) {
				verifier.Add(v.Object);
			}

			verifier.VerifiedEvent += (s, e) =>
			{
				eventCalledCount++;
			};

			//Act.
			var actual = verifier.Verify(doc.Object, DateTime.MaxValue);

			//Assert
			doc.Verify();
			structureVerifier.Verify();
			foreach (var v in mockVerifiers) {
				v.Verify();
			}

			Assert.Multiple(() =>
			{
				//署名が一つも無い場合はINVALID
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.VerificationResults.Count(), Is.EqualTo(6));
			});

			foreach (var act in actual.VerificationResults) {
				Assert.That(act.Status, Is.EqualTo(VerificationStatus.VALID));
			}

			Assert.That(eventCalledCount, Is.EqualTo(0), "event is called.");

			return;
		}

		[Test(Description = "個別検証でINVALIDでも継続すること。")]
		public void Verify_WithSomeVerifiierIsInvalid_ContinueToEnd()
		{
			//Arrange
			var config = new VerificationConfig();

			var structureVerifier = new Mock<IStructureVerifier>();
			var structureVerifierResult = new VerificationResult(VerificationStatus.VALID, null);
			structureVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>())).Returns(structureVerifierResult);

			var doc = new Mock<ISignedDocument>();
			doc.Setup(x => x.CreateStructureVerifier(config)).Returns(structureVerifier.Object);

			var mockVerifiers = new List<Mock<IVerifier>>();
			foreach (var i in Enumerable.Range(0, 5)) {

				var mockVerifier = new Mock<IVerifier>();

				if ((i % 2) == 0) {
					var mockResult = new VerificationResult(VerificationStatus.INVALID, null);
					var mockEventArgs = new VerifiedEventArgs(VerificationStatus.INVALID, "テスト検証", "エラー");

					mockVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>(), It.IsAny<DateTime>()))
						.Callback(() => mockVerifier.Raise(x => x.VerifiedEvent += null, mockEventArgs))
						.Returns(mockResult);
				}
				else {
					var mockResult = new VerificationResult(VerificationStatus.VALID, null);
					mockVerifier.Setup(x => x.Verify(It.IsAny<ISignedDocument>(), It.IsAny<DateTime>()))
						.Returns(mockResult);
				}

				mockVerifiers.Add(mockVerifier);
			}

			var eventCalledCount = 0;

			var verifier = new SignatureVerifier(config);
			verifier.Clear();
			foreach (var v in mockVerifiers) {
				verifier.Add(v.Object);
			}

			verifier.VerifiedEvent += (s, e) =>
			{
				eventCalledCount++;
			};

			//Act.
			var actual = verifier.Verify(doc.Object, DateTime.MaxValue);

			//Assert
			doc.Verify();
			structureVerifier.Verify();
			foreach (var v in mockVerifiers) {
				v.Verify();
			}

			Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
			Assert.Multiple(() =>
			{
				Assert.That(actual.VerificationResults.Count(), Is.EqualTo(6));
				Assert.That(actual.VerificationResults.Count(x => x.Status == VerificationStatus.VALID), Is.EqualTo(3));
				Assert.That(actual.VerificationResults.Count(x => x.Status == VerificationStatus.INVALID), Is.EqualTo(3));
			});

			Assert.That(eventCalledCount, Is.EqualTo(3), "event is called.");

			return;
		}
	}
}
