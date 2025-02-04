using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier
{
	/// <summary>
	/// 署名検証メインクラス
	/// </summary>
	public class SignatureVerifier
	{
		/// <summary>検証直後に発生するイベント</summary>
		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		private readonly VerificationConfig _config;
		private readonly IList<IVerifier> _verifiers = new List<IVerifier>();

		/// <summary>個別検証器リスト</summary>
		public IEnumerable<IVerifier> Verifiers => _verifiers;

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="config">検証オプション</param>
		public SignatureVerifier(VerificationConfig config)
		{
			_config = config;

			//既定の検証器を追加
			Add(new SigningCertificateVerifier(config));
			Add(new ReferenceVerifier(config));
			Add(new SignatureValueVerifier(config));
			Add(new SignatureTimeStampVerifier(config));
			Add(new ArchiveTimeStampVerifier(config));
		}


		/// <summary>
		/// 検証処理を実行する
		/// </summary>
		/// <param name="doc">検証対象ドキュメント</param>
		/// <param name="verificationTime">検証時間（現在時間：ローカルタイム）を指定する。</param>
		/// <returns>検証した結果を返却します。返却値の検証結果プロパティを確認してください。</returns>
		public SignatureVerificationResult Verify(ISignedDocument doc, DateTime verificationTime)
		{
			_logger.Debug("Verify start!!");

			var verifyResults = new ConcurrentBag<VerificationResult>();

			//構造検証
			var structureVerifier = doc.CreateStructureVerifier(_config);
			structureVerifier.VerifiedEvent += OnVerifiedEvent;

			var structureVerified = structureVerifier.Verify(doc);
			verifyResults.Add(structureVerified);

			structureVerifier.VerifiedEvent -= OnVerifiedEvent;
			if (structureVerified.Status != VerificationStatus.VALID) {

				return new SignatureVerificationResult(VerificationStatus.INVALID, verifyResults);
			}

			foreach (var verifier in _verifiers) {

				var verified = verifier.Verify(doc, verificationTime);
				verifyResults.Add(verified);
			}

			//既定の位置に署名要素が無い場合は強制的にエラー(電子処方箋以外の検証が入ってきたらこの処理は見直す)
			if (!doc.Signatures.Any(m => m.SourceType != SignatureSourceType.Unknown)) {
				return new SignatureVerificationResult(VerificationStatus.INVALID, verifyResults);
			}

			var lastStatis = verifyResults.Select(x => x.Status).ToConclusion();

			return new SignatureVerificationResult(lastStatis, verifyResults);
		}


		private void OnVerifiedEvent(object sender, VerifiedEventArgs eventArgs)
		{
			VerifiedEvent?.Invoke(sender, eventArgs);
		}

		/// <summary>
		/// 内部検証器を追加します。
		/// </summary>
		/// <param name="verifier">検証器</param>
		public void Add(IVerifier verifier)
		{
			_verifiers.Add(verifier);
			verifier.VerifiedEvent += OnVerifiedEvent;
		}

		/// <summary>
		/// 内部検証器をクリアします。
		/// </summary>
		public void Clear()
		{
			foreach (var verifier in _verifiers) {
				verifier.VerifiedEvent -= OnVerifiedEvent;
			}
			_verifiers.Clear();
		}

	}
}
