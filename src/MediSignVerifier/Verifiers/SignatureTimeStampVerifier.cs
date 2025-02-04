using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SignatureVerifier.Data;

namespace SignatureVerifier.Verifiers
{
	internal class SignatureTimeStampVerifier : IVerifier
	{
		public static readonly string Name = "署名タイムスタンプ";

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		private readonly VerificationConfig _config;
		private readonly IList<VerificationResultItem> _results;

		public SignatureTimeStampVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}


		public VerificationResult Verify(ISignedDocument doc, DateTime verificationTime)
		{
			_logger.Trace("署名タイムスタンプの検証を開始します。");

			//※調剤データは特別扱い
			if (doc.DocumentType == DocumentType.Dispensing) {
				//調剤から検証
				var dispSignature = doc.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.Dispensing);
				if (dispSignature != null) {
					VerifySignature(dispSignature, verificationTime);
				}

				//調剤内処方
				var dispPrescSignature = doc.Signatures.FirstOrDefault(m => m.SourceType == SignatureSourceType.DispPrescription);
				if (dispPrescSignature != null) {
					//調剤の署名タイムスタンプ時刻を使う

					DateTime? dispSigTSGenTime = null;
					try {
						// 内部処理が存在チェックも兼ねているので要素が無い場合に SignatureTimeStampValidationData から null を返却できなかった。
						dispSigTSGenTime = dispSignature?.SignatureTimeStampValidationData.TimeStampData.GenTime;
					}
					catch (Exception ex) {

						_logger.Debug(ex, "Ignore exceptions, return null.");
					}

					VerifySignature(dispPrescSignature, dispSigTSGenTime ?? verificationTime);
				}

				//その他(とりあえず検証)
				foreach (var signature in doc.Signatures.Where(m => m.SourceType == SignatureSourceType.Unknown)) {
					VerifySignature(signature, verificationTime);
				}
			}
			else {
				foreach (var signature in doc.Signatures) {
					VerifySignature(signature, verificationTime);
				}
			}

			var status = _results.Select(x => x.Status).ToConclusion();

			_logger.Debug($"署名タイムスタンプの検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private void VerifySignature(ISignature signature, DateTime verificationTime)
		{
			if (signature.ESLevel < ESLevel.T) {
				_logger.Info($"\"{signature.SourceType}\"の署名タイムスタンプの検証はスキップします。: ESLevel=\"{signature.ESLevel}\"");
				return;
			}

			_logger.Trace($"\"{signature.SourceType}\"の署名タイムスタンプの検証を開始します。: ESLevel=\"{signature.ESLevel}\"");

			//検証基準時刻
			//T～XL：現在時刻UTC
			DateTime? validDate = null;

			//-A：一番古いアーカイブスタンプの生成時刻
			if (signature.ESLevel == ESLevel.A) {
				try {
					validDate = signature.OldestArchiveTimeStampGenTime;
				}
				catch (Exception ex) {
					_logger.Error(ex);
				}

				if (validDate == null) {
					//エラーになった場合は検証中断でよい？
					DoPostVerification(VerificationStatus.INVALID, signature.SourceType, "検証基準時刻", "", "検証基準時刻の取得に失敗しました。");
					return;
				}
			}
			else {
				validDate = verificationTime.ToUniversalTime();
			}
			_logger.Info($"署名タイムスタンプ: 検証基準時刻=\"{validDate:O}\"");

			try {
				var id = signature.SignatureTimeStampValidationData.Id;

				// [検証要件] 5.5.3 XAdES - 署名タイムスタンプ - タイムスタンプトークンの検証
				// [検証要件] 5.5.3 XAdES - 署名タイムスタンプ - MessageImprint値
				signature.SignatureTimeStampValidationData.Validate(validDate.Value, _config,
					out TimeStampValidationExceptions errors);

				foreach (var error in errors) {
					_logger.Warn(error, "署名タイムスタンプ検証エラー");
					DoPostVerification(error.Status, signature.SourceType, error.ItemName, id, error.Message);
				}

				//タイムスタンプトークンの検証が成功
				if (!errors.Any(m => m.ItemName != TimeStampItemNames.MessageImprint)) {
					DoPostVerification(VerificationStatus.VALID, signature.SourceType, TimeStampItemNames.Token, id, null);
				}

				//MessageImprintの検証が成功
				if (!errors.Any(m => m.ItemName == TimeStampItemNames.MessageImprint)) {
					DoPostVerification(VerificationStatus.VALID, signature.SourceType, TimeStampItemNames.MessageImprint, id, null);
				}
			}
			catch (TimeStampConvertException ex) {
				_logger.Error(ex);
				//データ変換に失敗
				DoPostVerification(VerificationStatus.INVALID, signature.SourceType, ex.ItemName, "", ex.Message);
			}
			catch (Exception ex) {
				_logger.Error(ex);
				DoPostVerification(VerificationStatus.INVALID, signature.SourceType, "", "", "不明なエラー");
			}
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType sigType, string itemName, string mappedName, string message)
		{
			var item = new VerificationResultItem(status, sigType, Name, itemName, mappedName, message);
			_results.Add(item);

			if (status != VerificationStatus.VALID) {
				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}
	}
}
