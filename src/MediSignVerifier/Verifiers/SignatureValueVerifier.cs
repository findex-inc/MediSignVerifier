using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Verifiers
{
	/// <summary>
	/// 署名データ検証器
	/// </summary>
	internal class SignatureValueVerifier : IVerifier
	{
		public static readonly string Name = "署名データ";

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

#pragma warning disable IDE0052
		private readonly VerificationConfig _config;
#pragma warning restore IDE0052
		private readonly IList<VerificationResultItem> _results;

		public SignatureValueVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}

		public VerificationResult Verify(ISignedDocument doc, DateTime _)
		{
			_logger.Trace("署名データの検証を開始します。");

			foreach (var signature in doc.Signatures) {
				VerifySignature(signature);
			}

			var status = _results.Select(m => m.Status).ToConclusion();

			_logger.Debug($"署名データの検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private void VerifySignature(ISignature signature)
		{
			//ESLevelに関係なく検証が必要
			if (signature.ESLevel < ESLevel.BES) {
				_logger.Info($"\"{signature.SourceType}\"の署名データの検証はスキップします。: ESLevel=\"{signature.ESLevel}\"");
				return;
			}

			_logger.Trace($"\"{signature.SourceType}\"の署名データの検証を開始します。: ESLevel=\"{signature.ESLevel}\"");

			try {
				//署名値
				VerifySignatureValue(
					signature.SignatureValueValidationData,
					signature.SigningCertificateValidationData?.Certificate?.Value,
					signature.SourceType);

				//アルゴリズム
				VerifySignatureMethod(signature.SignatureValueValidationData, signature.SourceType);

				_logger.Trace($"\"{signature.SourceType}\"の署名データの検証を終了します。");
			}
			catch (Exception ex) {
				_logger.Error(ex);
				DoPostVerification(VerificationStatus.INVALID, signature.SourceType, "", "", "不明なエラー");
			}
		}

		private void VerifySignatureValue(SignatureValueValidationData data, byte[] sigCert, SignatureSourceType sigType)
		{
			// [検証要件] 5.5.3 XAdES - 署名データ - SignatureValue要素
			const string itemName = "SignatureValue要素";
			var status = VerificationStatus.VALID;
			string message = null;

			if (data.ConvertError == null) {
				try {
					//署名者証明書の公開鍵を取り出し
					var certificate = new X509Certificate(sigCert);
					var publicKey = certificate.GetPublicKey();

					//公開鍵を使ってSignatureMethodの署名アルゴリズムで署名値SignatureValueを復号
					//data.SignatureMethod をBouncyCastleがわかる形式に変換する　DerObjectIdentifier

					var sigId = data.SignatureMethod.ToBCMechanism();
					var signer = SignerUtilities.GetSigner(sigId);
					signer.Init(false, publicKey);
					signer.BlockUpdate(data.TargetValue, 0, data.TargetValue.Length);

					//SignedInfo要素を正規化した値と復号結果が同じであるか
					var result = signer.VerifySignature(data.SignatureValue);
					if (!result) {
						status = VerificationStatus.INVALID;
						message = "復号した署名値と計算した署名値が一致しません。";
					}
				}
				catch (Exception ex) {
					_logger.Error(ex);
					status = VerificationStatus.INVALID;
					message = "署名値の復号に失敗しました。";
				}
			}
			else {
				//変換エラーがあった
				_logger.Error(data.ConvertError);
				status = VerificationStatus.INVALID;
				message = data.ConvertError.Message;
			}

			DoPostVerification(status, sigType, itemName, data.Id, message);
		}

		private void VerifySignatureMethod(SignatureValueValidationData data, SignatureSourceType sigType)
		{
			// [検証要件] 5.5.3 XAdES - 署名データ - SignatureMethod要素
			const string itemName = "SignatureMethod要素";
			var status = VerificationStatus.VALID;
			string message = null;

			var result = SecurityCryptoXmlHelper.IsSupportedSignatureAlgorithm(data.SignatureMethod);
			if (!result) {
				status = VerificationStatus.INVALID;
				message = "サポートされていない署名アルゴリズムが指定されています。";
			}

			DoPostVerification(status, sigType, itemName, data.Id, message);
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType sigType, string itemName, string mappedName, string message)
		{
			var item = new VerificationResultItem(status, sigType, Name, itemName, mappedName, message);
			_results.Add(item);

			if (status != VerificationStatus.VALID) { //or == INVALID
				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}
	}
}
