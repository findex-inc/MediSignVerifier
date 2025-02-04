using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SignatureVerifier.Data;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Verifiers
{
	/// <summary>
	/// 参照データ検証器
	/// </summary>
	internal class ReferenceVerifier : IVerifier
	{
		public static readonly string Name = "参照データ";

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

#pragma warning disable IDE0052
		private readonly VerificationConfig _config;
#pragma warning restore IDE0052
		private readonly IList<VerificationResultItem> _results;

		public ReferenceVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}

		public VerificationResult Verify(ISignedDocument doc, DateTime _)
		{
			_logger.Trace("参照データの検証を開始します。");

			foreach (var signature in doc.Signatures) {
				VerifySignature(signature);
			}

			var status = _results.Select(m => m.Status).ToConclusion();

			_logger.Debug($"参照データの検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private void VerifySignature(ISignature signature)
		{
			//ESLevelに関係なく検証が必要
			if (signature.ESLevel < ESLevel.BES) {
				_logger.Info($"\"{signature.SourceType}\"の参照データの検証はスキップします。: ESLevel=\"{signature.ESLevel}\"");
				return;
			}

			_logger.Trace($"\"{signature.SourceType}\"の参照データの検証を開始します。: ESLevel=\"{signature.ESLevel}\"");

			try {
				foreach (var reference in signature.ReferenceValidationData) {

					//ダイジェスト
					VerifyDigestValue(reference, signature.SourceType);

					//アルゴリズム
					VerifyDigestMethod(reference, signature.SourceType);
				}

				_logger.Trace($"\"{signature.SourceType}\"の参照データの検証を終了します。");
			}
			catch (Exception ex) {
				_logger.Error(ex);
				DoPostVerification(VerificationStatus.INVALID, signature.SourceType, -1, "", "", "不明なエラー");
			}

		}

		private void VerifyDigestValue(ReferenceValidationData data, SignatureSourceType sigType)
		{
			// [検証要件] 5.5.3 XAdES - 参照データ - Reference要素
			const string itemName = "Reference要素";
			var status = VerificationStatus.VALID;
			string message = null;

			if (data.ConvertError == null) {
				try {
					var result = data.DigestValue.SequenceEqual(data.CalculatedValue);
					if (!result) {
						status = VerificationStatus.INVALID;
						message = "計算したハッシュ値とDigestValueの値が一致しません。";
					}
				}
				catch (ArgumentNullException ex) {
					_logger.Error(ex);
					status = VerificationStatus.INVALID;
					message = ex.Message;
				}
			}
			else {
				//Referenceへの変換エラーがあった
				_logger.Error(data.ConvertError);
				status = VerificationStatus.INVALID;
				message = data.ConvertError.Message;
			}

			DoPostVerification(status, sigType, data.DispIndex, itemName, data.Id, message);
		}

		private void VerifyDigestMethod(ReferenceValidationData data, SignatureSourceType sigType)
		{
			// [検証要件] 5.5.3 XAdES - 参照データ - DigestMethod要素
			const string itemName = "DigestMethod要素";
			var status = VerificationStatus.VALID;
			string message = null;

			var result = SecurityCryptoXmlHelper.IsSupportedHashAlgorithm(data.DigestMethod);
			if (!result) {
				status = VerificationStatus.INVALID;
				message = "サポートされていないダイジェストアルゴリズムが指定されています。";
			}

			DoPostVerification(status, sigType, data.DispIndex, itemName, data.Id, message);
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType sigType, int index, string itemName, string mappedName, string message)
		{
			var item = new VerificationResultItem(status, sigType, Name, index, itemName, mappedName, message);
			_results.Add(item);

			if (status != VerificationStatus.VALID) { //or == INVALID
				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}
	}
}
