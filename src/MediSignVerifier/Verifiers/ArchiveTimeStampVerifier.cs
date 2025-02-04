using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Verifiers
{
	internal class ArchiveTimeStampVerifier : IVerifier
	{
		public static readonly string Name = "アーカイブタイムスタンプ";

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly VerificationConfig _config;
		private readonly IList<VerificationResultItem> _results;

		public ArchiveTimeStampVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}

		public VerificationResult Verify(ISignedDocument doc, DateTime verificationTime)
		{
			_logger.Trace("アーカイブタイムスタンプの検証を開始します。");

			foreach (var signature in doc.Signatures) {

				VerifySignature(signature, verificationTime);
			}

			var status = _results.Select(x => x.Status).ToConclusion();

			_logger.Debug($"アーカイブタイムスタンプの検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private void VerifySignature(ISignature signature, DateTime verificationTime)
		{
			if (signature.ESLevel < ESLevel.A) {
				_logger.Info($"\"{signature.SourceType}\"のアーカイブタイムスタンプの検証はスキップします。: ESLevel=\"{signature.ESLevel}\"");
				return;
			}

			_logger.Trace($"\"{signature.SourceType}\"のアーカイブタイムスタンプの検証を開始します。: ESLevel=\"{signature.ESLevel}\"");

			try {
				if (!signature.ArchiveTimeStampValidationData.Any()) {
					//-AにもかかわらずATS要素が無かった
					DoPostVerification(VerificationStatus.INVALID, signature.SourceType, -1, "", "", "アーカイブタイムスタンプが見つかりませんでした。");
					return;
				}

				foreach (var ats in signature.ArchiveTimeStampValidationData) {

					if (ats.ConvertError != null) {
						_logger.Error(ats.ConvertError);
						//データ変換に失敗
						DoPostVerification(VerificationStatus.INVALID, signature.SourceType, ats.DispIndex,
							ats.ConvertError.ItemName, ats.Id, ats.ConvertError.Message);
						continue;
					}

					//検証基準時刻：次世代のATS生成時刻 or (最新の場合は)検証時刻UTC
					DateTime? validDate = null;
					var nextGenAts = signature.ArchiveTimeStampValidationData.FirstOrDefault(m => m.Index == ats.Index + 1);
					if (nextGenAts == null) {
						validDate = verificationTime.ToUniversalTime();
					}
					else {
						//次世代のATS要素があった
						if (nextGenAts.ConvertError == null) {
							try {
								validDate = nextGenAts.ValidationData.TimeStamp.ToTimeStampToken().TimeStampInfo.GenTime;
							}
							catch (Exception ex) {
								//TimeStampTokenの変換に失敗
								_logger.Error(ex);
							}
						}

						if (validDate == null) {
							//次世代タイムスタンプ変換に失敗している可能性がある
							DoPostVerification(VerificationStatus.INVALID, signature.SourceType, ats.DispIndex,
								"検証基準時刻", ats.Id, "次世代アーカイブタイムスタンプ生成時刻の取得に失敗しました。");
							continue;
						}
					}
					_logger.Info($"アーカイブタイムスタンプ: 検証基準時刻=\"{validDate:O}\"");

					// [検証要件] 5.5.3 XAdES - アーカイブタイムスタンプ - タイムスタンプトークンの検証
					// [検証要件] 5.5.3 XAdES - アーカイブタイムスタンプ - MessageImprint値
					ats.ValidationData.Validate(validDate.Value, _config, out TimeStampValidationExceptions errors);

					foreach (var error in errors) {
						_logger.Warn(error, $"アーカイブタイムスタンプ検証エラー[{ats.DispIndex}]");
						DoPostVerification(error.Status, signature.SourceType, ats.DispIndex, error.ItemName, ats.Id, error.Message);
					}

					//タイムスタンプトークンの検証が成功
					if (!errors.Any(m => m.ItemName != TimeStampItemNames.MessageImprint)) {
						DoPostVerification(VerificationStatus.VALID, signature.SourceType, ats.DispIndex, TimeStampItemNames.Token, ats.Id, null);
					}

					//MessageImprintの検証が成功
					if (!errors.Any(m => m.ItemName == TimeStampItemNames.MessageImprint)) {
						DoPostVerification(VerificationStatus.VALID, signature.SourceType, ats.DispIndex, TimeStampItemNames.MessageImprint, ats.Id, null);
					}
				}
			}
			catch (Exception ex) {
				_logger.Error(ex);
				DoPostVerification(VerificationStatus.INVALID, signature.SourceType, -1, "", "", "不明なエラー");
			}
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType sigType, int index, string itemName, string mappedName, string message)
		{
			var item = new VerificationResultItem(status, sigType, Name, index, itemName, mappedName, message);
			_results.Add(item);

			if (status != VerificationStatus.VALID) {
				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}
	}
}
