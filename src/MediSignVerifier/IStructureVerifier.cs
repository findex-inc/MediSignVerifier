using System;

namespace SignatureVerifier
{
	/// <summary>
	/// 構造検証インターフェース
	/// </summary>
	public interface IStructureVerifier
	{
		/// <summary>
		/// 検証要件ごとに検証直後に発生
		/// </summary>
		event EventHandler<VerifiedEventArgs> VerifiedEvent;

		/// <summary>
		/// 検証実行
		/// </summary>
		/// <param name="doc">対象となるドキュメント</param>
		/// <returns>検証した結果を返却します。返却値の検証結果プロパティを確認してください。</returns>
		VerificationResult Verify(ISignedDocument doc);

	}
}
