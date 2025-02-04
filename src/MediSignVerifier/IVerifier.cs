
using System;

namespace SignatureVerifier
{
	/// <summary>
	/// 検証器インターフェース
	/// </summary>
	public interface IVerifier
	{
		/// <summary>
		/// 検証要件ごとに検証直後に発生
		/// </summary>
		event EventHandler<VerifiedEventArgs> VerifiedEvent;

		/// <summary>
		/// 検証実行
		/// </summary>
		/// <param name="doc">検証対象ドキュメント</param>
		/// <param name="verificationTime">検証基準時刻</param>
		/// <returns>検証した結果を返却します。返却値の検証結果プロパティを確認してください。</returns>
		VerificationResult Verify(ISignedDocument doc, DateTime verificationTime);

	}
}
