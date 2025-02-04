using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Org.BouncyCastle.Security.Certificates;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Verifiers
{
	internal class SigningCertificateVerifier : IVerifier
	{
		public static readonly string Name = "署名者証明書";

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private readonly VerificationConfig _config;
		private readonly IList<VerificationResultItem> _results = new List<VerificationResultItem>();


		public SigningCertificateVerifier(VerificationConfig config)
		{
			_config = config;
		}


		public VerificationResult Verify(ISignedDocument doc, DateTime verificationTime)
		{
			_logger.Trace("署名者証明書の検証を開始します。");

			foreach (var signature in doc.Signatures) {

				VerifySignature(signature, verificationTime);
			}

			var status = _results.Select(x => x.Status).ToConclusion();

			_logger.Debug($"署名者証明書の検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private static VerificationResultItem CreateResultItem(VerificationStatus status, SignatureSourceType sigType, string itemName, string mappedName, string message = null)
		{
			return new VerificationResultItem(status, sigType, Name, itemName, mappedName, message);
		}

		private void DoPostVerification(VerificationResultItem item)
		{
			_results.Add(item);

			if (item.Status != VerificationStatus.VALID) {

				_logger.Warn(item.Message);

				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(item.Status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}

		private void VerifySignature(ISignature signature, DateTime verificationTime)
		{

			if (signature.ESLevel < ESLevel.BES) {
				_logger.Info($"\"{signature.SourceType}\"の署名者証明書の検証はスキップします。: ESLevel=\"{signature.ESLevel}\"");
				return;
			}

			_logger.Trace($"\"{signature.SourceType}\"の署名者証明書の検証を開始します。: ESLevel=\"{signature.ESLevel}\"");

			var verifiers = new Func<VerificationResultItem>[]
			{
				() => VerifyCertificateLocation(signature),
				() => VerifyCertificateDecoded(signature),
				() => VerifyCertificateConsistency(signature),
				() => VerifyHPKICertificate(signature),
				() => VerifyCertificatePath(signature, verificationTime),
			};

			var results = new List<VerificationResultItem>();

			foreach (var verifier in verifiers) {

				var result = verifier.Invoke();

				results.Add(result);
				DoPostVerification(result);

				if (result.Status != VerificationStatus.VALID) {

					break;
				}
			}

			var status = results.Select(x => x.Status).ToConclusion();
			_logger.Debug($"\"{signature.SourceType}\"の署名者証明書: Status={status}");

			_logger.Trace($"\"{signature.SourceType}\"の署名者証明書の検証を終了します。");

			return;
		}


		private VerificationResultItem VerifyCertificateLocation(ISignature signature)
		{
			// [検証要件] 5.5.3 XAdES の検証要件 - 署名者証明書 - 証明書の指定確認

			const string itemName = "証明書の指定確認";

			var sourceType = signature.SourceType;
			var data = signature.SigningCertificateValidationData;

			if (data == null) {

				var invalid = CreateResultItem(VerificationStatus.INVALID, sourceType, itemName, null,
					"署名者証明書が見つかりません。");

				return invalid;
			}

			var success = CreateResultItem(VerificationStatus.VALID, sourceType, itemName, null);

			return success;
		}

		private VerificationResultItem VerifyCertificateDecoded(ISignature signature)
		{
			// [検証要件] 5.5.3 XAdES の検証要件 - 署名者証明書 - 証明書の実体確認

			const string itemName = "証明書の実体確認";

			var sourceType = signature.SourceType;
			var data = signature.SigningCertificateValidationData;

			try {

				data.Certificate.Value.ToX509Certificate();

			}
			catch (Exception ex) when (ex is NullReferenceException || ex is ArgumentException || ex is CertificateParsingException) {

				_logger.Warn(ex);

				var invalid = CreateResultItem(VerificationStatus.INVALID, sourceType, itemName, null,
					"署名者証明書の実体が確認できません。");

				return invalid;
			}

			var success = CreateResultItem(VerificationStatus.VALID, sourceType, itemName, null);

			return success;
		}

		private VerificationResultItem VerifyCertificateConsistency(ISignature signature)
		{
			// [検証要件] 5.5.3 XAdES の検証要件 - 署名者証明書 - 証明書の一致確認

			const string itemName = "証明書の一致確認";

			var sourceType = signature.SourceType;
			var signedCert = signature.SigningCertificateValidationData;

			var cert = signedCert.Certificate.Value.ToX509Certificate();

			if (signedCert.IssuerSerial != null) {

				var issuer = signedCert.IssuerSerial.IssuerName?.ToX509Name();
				var serial = signedCert.IssuerSerial.SerialNumber?.ToBigInteger();

				if (issuer == null || serial == null
					|| !cert.IssuerDN.Equivalent(issuer, false)
					|| !cert.SerialNumber.Equals(serial)) {

					var invalid = CreateResultItem(VerificationStatus.INVALID, sourceType, itemName, signedCert.Certificate.Id,
						"署名者証明書の参照と実体が一致しません。" +
						$"Issuer=\"{issuer}\", SerialNumber=\"{serial}\", " +
						$"Certificate={cert.Report()}");

					return invalid;
				}
			}

			if (signedCert.CertDigest != null) {

				var registed = signedCert.CertDigest.DigestValue;
				var digestMethod = signedCert.CertDigest.DigestMethod;

				var algorithm = SecurityCryptoXmlHelper.CreateHashAlgorithmFromName(digestMethod)
					.ToDerObjectIdentifier();

				var calclated = cert.CalculateDigest(algorithm);

				if (!registed.SequenceEqual(calclated)) {

					var invalid = CreateResultItem(VerificationStatus.INVALID, sourceType, itemName, signedCert.Certificate.Id,
						"署名者証明書の参照と実体が一致しません。" +
						$"DigestMethod=\"{digestMethod}\", DigestValue=\"{registed?.ToBase64String()}\", " +
						$"Certificate={cert.Report()}");

					return invalid;
				}
			}

			var success = CreateResultItem(VerificationStatus.VALID, sourceType, itemName, null);

			return success;
		}

		private VerificationResultItem VerifyHPKICertificate(ISignature signature)
		{
			// [検証要件] 5.5.3 XAdES の検証要件 - 署名者証明書 - 証明書のパス構築とパス検証
			const string itemName = "HPKI固有項目の確認";

			var sourceType = signature.SourceType;
			var signedCert = signature.SigningCertificateValidationData;

			var cert = signedCert.Certificate.Value.ToX509Certificate();

			if (_config.HPKIValidationEnabled) {

				var keyUsage = cert.GetKeyUsage();
				var nonRepudiationIndex = 1;

				var nonRepudiation = (keyUsage?[nonRepudiationIndex] ?? false);
				if (!nonRepudiation) {
					var invalid = CreateResultItem(VerificationStatus.INVALID, sourceType, itemName, signedCert.Certificate.Id,
							"署名者証明書の keyUsage の nonRepudiation が設定されていません。" +
							$"Certificate={cert.Report()}");
					return invalid;
				}
			}

			var success = CreateResultItem(VerificationStatus.VALID, sourceType, itemName, null);

			return success;
		}


		private VerificationResultItem VerifyCertificatePath(ISignature signature, DateTime verificationTime)
		{
			// [検証要件] 5.5.3 XAdES の検証要件 - 署名者証明書 - 証明書のパス構築とパス検証

			const string itemName = "証明書のパス構築とパス検証";

			var sourceType = signature.SourceType;
			var targetCertificate = signature.SigningCertificateValidationData.Certificate;
			var validationData = signature.SigningCertificateValidationData.PathValidationData;

			var baseTime = signature.ESLevel < ESLevel.T
				? verificationTime.ToUniversalTime()
				: signature.SignatureTimeStampGenTime ?? verificationTime.ToUniversalTime();
			_logger.Info($"署名者証明書: 検証基準時刻=\"{baseTime:O}\"");

			try {

				validationData.Validate(baseTime, targetCertificate, _config);

			}
			catch (CertificatePathValidationException ex) {

				_logger.Warn(ex);

				var invalid = CreateResultItem(ex.Status, sourceType, itemName, targetCertificate?.Id,
					ex.Message);

				return invalid;
			}

			var success = CreateResultItem(VerificationStatus.VALID, sourceType, itemName, null);

			return success;
		}

	}
}
