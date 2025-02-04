using System;
using NUnit.Framework;
using SignatureVerifier.Reports;
using static SignatureVerifier.Verifiers.CertificatePathValidationDataTests.Fixtures;

namespace SignatureVerifier.Verifiers
{
	internal partial class CertificatePathValidationDataTests
	{
		private readonly Fixtures _fixtures = new Fixtures();

		[Test(Description = "5.7 証明書の検証要件 - 証明書パス構築 - VALID")]
		public void Validate()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, _) = _fixtures.GetTestPathValidationData(utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			pathData.Validate(utcDateTime, eeCert, vconfig);

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
		}


		[Test(Description = "5.7 証明書の検証要件 - 証明書パス構築 - データ構造の正当性確認 - INVALID")]
		[SetUICulture("")]
		public void Validate_WithInvalidStructures([Values] InvalidStructureType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForInvalidStructure(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForInvalidStructure(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Is.EqualTo(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Does.StartWith(expected.InnerException.Message));
			});
		}


		[Test(Description = "5.7 証明書の検証要件 - 証明書パス構築 - 拡張領域における制約の確認 - INVALID")]
		public void Validate_WithInvalidConstraints([Values] InvalidConstraintType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForInvalidConstraint(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForInvalidConstraint(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Does.StartWith(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));

				Assert.That(actual.InnerException.InnerException, Is.TypeOf(expected.InnerException.InnerException.GetType()));
				Assert.That(actual.InnerException.InnerException.Message, Is.EqualTo(expected.InnerException.InnerException.Message));
			});
		}


		[Test(Description = "5.7 証明書の検証要件 - 証明書パス構築 - 証明書パス構築の確認 - INDETERMINATE")]
		public void Validate_WithInvalidPathBuild([Values] InvalidPathBuildType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForInvalidPathBuild(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForInvalidPathBuild(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Is.EqualTo(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));

			});
		}

		[Test(Description = "5.7 証明書の検証要件 - 証明書パス検証 - 証明書の改ざん確認 - INVALID")]
		public void Validate_WithInvalidSignature([Values] InvalidSigunatureType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForInvalidSigunature(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForInvalidSigunature(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Is.EqualTo(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));

				Assert.That(actual.InnerException.InnerException, Is.TypeOf(expected.InnerException.InnerException.GetType()));
				Assert.That(actual.InnerException.InnerException.Message, Is.EqualTo(expected.InnerException.InnerException.Message));
			});
		}

		[Test(Description = "5.7 証明書の検証要件 - 証明書パス検証 - 失効確認 - INVALID")]
		[SetCulture("")]
		public void Validate_WithRevoked([Values] RevokedType type)
		{
			// Use InvariantCulture because datetime format in message.

			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForRevoked(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForRevoked(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Does.StartWith(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));

			});
		}


		[Test(Description = "5.7 証明書の検証要件 - 証明書パス検証 - 失効確認 - VALID")]
		public void Validate_WithPresentButNotRevokedRevoked()
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, _) = _fixtures.GetTestDataForNotRevoked(utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			pathData.Validate(utcDateTime, eeCert, vconfig);

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

		}


		[Test(Description = "5.7 証明書の検証要件 - 証明書パス検証 - 有効期間の確認 - INVALID")]
		public void Validate_WithExpired([Values] ExpiredType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForExpired(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForExpired(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Does.StartWith(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));

				Assert.That(actual.InnerException.InnerException, Is.TypeOf(expected.InnerException.InnerException.GetType()));
				Assert.That(actual.InnerException.InnerException.Message, Is.EqualTo(expected.InnerException.InnerException.Message));
			});
		}


		[Test(Description = "5.7 証明書の検証要件 - 失効情報 - 失効情報の妥当性確認 - INDETERMINATE")]
		public void Validate_WithInvalidRevokationExpired([Values] RevokationExpiredType type)
		{
			//Arrange
			var utcDateTime = DateTime.Parse("2008-09-04T05:49:10").ToUniversalTime();
			var (eeCert, pathData, trust) = _fixtures.GetTestDataForRevokationExpired(type, utcDateTime);

			var vconfig = new VerificationConfig();
			var rconfig = new ReportConfig();

			//Act.
			var actual = Assert.Throws<CertificatePathValidationException>(
				() => pathData.Validate(utcDateTime, eeCert, vconfig));

			TestContext.WriteLine(TestData.ToJson(pathData.ToReportCertificates(utcDateTime, eeCert, rconfig), DateTimeZoneHandling.RoundtripKind));

			//Assert
			var expected = _fixtures.GetExpectedExceptionForRevokationExpired(type);

			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(expected.Status));
				Assert.That(actual.Message, Does.StartWith(expected.Message));

				Assert.That(actual.InnerException, Is.TypeOf(expected.InnerException.GetType()));
				Assert.That(actual.InnerException.Message, Is.EqualTo(expected.InnerException.Message));
			});
		}

	}
}
