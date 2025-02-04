using System.Collections.Generic;
using System.Linq;

namespace SignatureVerifier
{
	/// <summary>
	/// 全体の検証結果
	/// </summary>
	public class SignatureVerificationResult
	{
		/// <summary>全体の検証結果</summary>
		public VerificationStatus Status { get; }

		/// <summary>検証器ごとの検証結果リスト</summary>
		public IEnumerable<VerificationResult> VerificationResults { get; }

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">全体の検証結果</param>
		/// <param name="verificationResults">検証器ごとの検証結果リスト</param>
		public SignatureVerificationResult(VerificationStatus status, IEnumerable<VerificationResult> verificationResults)
		{
			Status = status;
			VerificationResults = verificationResults;
		}

		/// <summary>
		/// 指定した検証対象の検証結果を取得します。
		/// </summary>
		/// <param name="source">結果を出力した検証対象を指定します。</param>
		/// <returns>検証対象ごとの検証結果を返却します。</returns>
		public VerificationResult GetVerificationResult(string source)
		{
			return VerificationResults.FirstOrDefault(m => m.Source == source);
		}
	}
}
