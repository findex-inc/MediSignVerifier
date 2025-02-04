using System;
using System.Linq;
using NLog;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Verifiers
{
	internal static class TimeStampValidationDataExtensions
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// タイムスタンプの検証
		/// </summary>
		/// <param name="validationData">タイムスタンプ検証データ</param>
		/// <param name="validDate">検証基準時刻</param>
		/// <param name="config">検証オプション</param>
		/// <param name="errors">[out]検証エラーリスト</param>
		/// <exception cref="TimeStampValidationException"></exception>
		public static void Validate(this TimeStampValidationData validationData,
			DateTime validDate,
			VerificationConfig config,
			out TimeStampValidationExceptions errors)
		{
			// [検証要件] 5.6.1 タイムスタンプ - データ構造 - データ構造の正当性確認
			CmsSignedData cmsData;
			try {
				//データ構造の正当性確認
				cmsData = validationData.TimeStamp.ToCmsSignedData();
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampValidationException("データ構造の正当性確認",
					"データ構造が不正です。", ex);
			}

			// [検証要件] 5.6.1 タイムスタンプ - データ構造 - CMSデータ形式の確認
			//ContentTypeがsigned-dataであること
			if (!cmsData.IsSignedData()) {
				//続行不可
				throw new TimeStampValidationException("CMSデータ形式の確認",
					$"ContentTypeが不正です。({cmsData.GetContentType()})");
			}

			// [検証要件] 5.6.1 タイムスタンプ - データ構造 - 署名対象データ形式の確認
			//eContentTypeがTSTInfoであること
			if (!cmsData.IsTstInfo()) {
				//続行不可
				throw new TimeStampValidationException("署名対象データ形式の確認",
					$"eContentTypeが不正です。({cmsData.GetSignedContentType()})");
			}

			TimeStampToken tsToken;
			SignerInformation tsaSignerInfo;
			try {
				//TspValidationException, ArgumentException,TspException..
				tsToken = new TimeStampToken(cmsData);
				tsaSignerInfo = tsToken.GetSignerInfo();
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampValidationException("データ形式の確認",
					"タイムスタンプ形式への変換に失敗しました。", ex);
			}

			errors = new TimeStampValidationExceptions();
			var tsData = new TimeStampData
			{
				ValidDate = validDate,
				GenTime = tsToken.TimeStampInfo.GenTime
			};

			//TSA証明書
			ValidateCertificate(validationData, validDate, errors, config);

			//TSAの署名
			ValidateSignature(tsToken, tsaSignerInfo, errors, tsData);

			//MessageImprint
			ValidateMessageImprint(tsToken, validationData.TargetValue, errors, tsData);

			validationData.SetTimeStampData(tsData);
		}

		private static void ValidateCertificate(TimeStampValidationData validationData, DateTime validDate, TimeStampValidationExceptions errors, VerificationConfig config)
		{
			// [検証要件] 5.6.1 タイムスタンプ - TSA証明書 - 証明書のパス構築とパス検証
			string itemName = TimeStampItemNames.TSACert;

			//変換エラーがあった(検証はスキップ？)
			if (validationData.CertErrors?.Any() == true) {
				errors.AddRange(validationData.CertErrors.Select(e => new TimeStampValidationException(itemName, e.Message, e.InnerException)));
				return;
			}

			try {
				validationData.CertificateValidationData.Validate(validDate, validationData.TsaCertificate, config, new TSACertChecker());
			}
			catch (CertificatePathValidationException ex) {
				errors.Add(new TimeStampValidationException(ex.Status, itemName, ex.Message, ex));
			}
			catch (Exception ex) {
				errors.Add(new TimeStampValidationException(itemName, "証明書のパス検証に失敗しました。", ex));
			}

			//鍵拡張利用目的に id-kp-timeStamping かつ criticalが存在していること
			try {
				TspUtil.ValidateCertificate(validationData.TsaCertificate.Value.ToX509Certificate());
			}
			catch (Exception ex) when (ex is ArgumentException || ex is TspValidationException) {
				errors.Add(new TimeStampValidationException(itemName, "鍵拡張利用目的にid-kp-timeStamping(critical)が存在しません。", ex));
			}
			catch (Exception ex) {
				/*読み捨て*/
				_logger.Debug(ex, "Ignore exceptions.");
			}

			//TODO [optional]鍵利用目的に digitalSignatureもしくは／かつ nonRepdiationがあること
		}

		private static void ValidateSignature(TimeStampToken tsToken, SignerInformation tsaSignerInfo, TimeStampValidationExceptions errors, TimeStampData tsData)
		{
			string itemName = TimeStampItemNames.TSASignature;
			X509Certificate signerCert = null;

			try {
				signerCert = tsToken.GetSignerCertificate();

				//BouncyCastleの検証を一通りかけてみる
				tsToken.Validate(signerCert);
			}
			catch (Exception ex) {
				//検証失敗。
				errors.Add(new TimeStampValidationException(itemName, "署名の検証に失敗しました。", ex));
			}

			//個別検証をかけてどこでエラーになったかが可能なかぎりわかるように

			// [検証要件] 5.6.1 タイムスタンプ - TSAの署名 - digestAlgorithmフィールドの有効性確認
			//signerInfoのdigestAlgorithm
			var signerDigestAlg = tsaSignerInfo.GetDigestAlgorithm();
			tsData.SignerDigestAlgorithm = signerDigestAlg.DigestAlgorithmName();
			if (!signerDigestAlg.IsSupportedDigestAlgorithm()) {
				errors.Add(new TimeStampValidationException(itemName,
					"サポートされていないダイジェストアルゴリズムが指定されています。"));
			}

			// [検証要件] 5.6.1 タイムスタンプ - TSAの署名 - MessageDigest属性の一致確認
			//signerInfoのMessageDigest(signerInformation.Verifyの後)
			try {
				tsData.MessageDigestValue = tsToken.GetMessageDigest();
				tsData.MessageCalculatedValue = tsaSignerInfo.GetContentDigest();
				if (!tsData.IsEqualsMessageDigest()) {
					errors.Add(new TimeStampValidationException(itemName,
						"計算したハッシュ値とMessageDigestの値が一致しません。"));
				}
			}
			catch (Exception ex) {
				errors.Add(new TimeStampValidationException(itemName,
					"MessageDigestの検証に失敗しました。", ex));
			}

			// [検証要件] 5.6.1 タイムスタンプ - TSAの署名 - SigningCertificate属性におけるTSA証明書のハッシュ値の一致確認
			//SigningCertificateのハッシュ値
			try {
				var certID = tsToken.GetCertID();
				tsData.CertificateDigestAlgorithm = certID.HashAlgorithm.DigestAlgorithmName();
				tsData.CertificateHashValue = certID.CertHash;
				tsData.CertificateCalculatedValue = signerCert.CalculateDigest(certID.HashAlgorithm);
				if (!tsData.IsEqualsCertificateHash()) {
					errors.Add(new TimeStampValidationException(itemName,
						"計算したハッシュ値とSigningCertificateのハッシュ値が一致しません。"));
				}
			}
			catch (Exception ex) {
				errors.Add(new TimeStampValidationException(itemName,
					"SigningCertificateの検証に失敗しました。", ex));
			}

			// [検証要件] 5.6.1 タイムスタンプ - TSAの署名 - signatureAlgorithmフィールドの有効性確認
			//signerInfoのsignatureAlgorithm
			var signerSignatureAlg = tsaSignerInfo.GetSignatureAlgorithm();
			tsData.SignerSignatureAlgorithm = signerSignatureAlg.SignatureAlgorithmName();
			if (!signerSignatureAlg.IsSupportedSignatureAlgorithm()) {
				errors.Add(new TimeStampValidationException(itemName,
					"サポートされていない署名アルゴリズムが指定されています。"));
			}

			// [検証要件] 5.6.1 タイムスタンプ - TSAの署名 - TSA証明書(公開鍵)による署名値の有効性確認
			//署名値
			try {
				//署名値が一致しなかった場合にfalseそれ以外は例外
				if (!tsaSignerInfo.Verify(signerCert)) {
					errors.Add(new TimeStampValidationException(itemName,
						"復号した署名値と計算した署名値が一致しません。"));
				}
			}
			catch (Exception ex) {
				/*読み捨て*/
				_logger.Debug(ex, "Ignore exceptions.");
			}
		}

		private static void ValidateMessageImprint(TimeStampToken tsToken, byte[] targetValue, TimeStampValidationExceptions errors, TimeStampData tsData)
		{
			string itemName = TimeStampItemNames.MessageImprint;

			// [検証要件] 5.6.1 タイムスタンプ - タイムスタンプ対象データ - hashAlgorithmフィールドの有効性確認
			//MessageImprintのhashAlgorithm
			var hashAlg = tsToken.TimeStampInfo.GetMessageImprintAlgorithm();
			tsData.HashAlgorithm = hashAlg.DigestAlgorithmName();
			if (!hashAlg.IsSupportedDigestAlgorithm()) {
				errors.Add(new TimeStampValidationException(itemName,
					"サポートされていないダイジェストアルゴリズムが指定されています。"));
			}

			// [検証要件] 5.6.1 タイムスタンプ - タイムスタンプ対象データ - タイムスタンプ対象データとの整合性確認
			//MessageImrpintのhashMessage
			try {
				tsData.HashValue = tsToken.TimeStampInfo.GetMessageImprintDigest();
				tsData.CalculatedValue = targetValue.CalculateDigest(tsToken.TimeStampInfo.GetMessageImprintAlgorithm());
				if (!tsData.IsEqualsTimeStampHash()) {
					errors.Add(new TimeStampValidationException(itemName,
						"計算したハッシュ値とhashMessageの値が一致しません。"));
				}
			}
			catch (Exception ex) {
				errors.Add(new TimeStampValidationException(itemName,
					"hashMessageの検証に失敗しました。", ex));
			}
		}

		class TSACertChecker : PkixCertPathChecker
		{
			public override void Init(bool forward)
			{
			}

			public override bool IsForwardCheckingSupported()
			{
				return true;
			}

			public override ISet GetSupportedExtensions()
			{
				return null;
			}

			public override void Check(X509Certificate cert, ISet unresolvedCritExts)
			{
				//TSAの場合は、タイムスタンプのVerifyで鍵拡張利用目的をチェックを行っているので削除でよいはず
				unresolvedCritExts.Remove(X509Extensions.ExtendedKeyUsage.Id);
			}
		}
	}
}
