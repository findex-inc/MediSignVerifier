using System.Collections.Generic;
using System.Linq;

namespace SignatureVerifier
{
	/// <summary>
	/// 検証器ごとの検証結果
	/// </summary>
	public class VerificationResult
	{
		/// <summary>検証対象</summary>
		public string Source { get; }

		/// <summary>検証結果</summary>
		public VerificationStatus Status { get; }

		/// <summary>個別検証結果リスト</summary>
		public IEnumerable<VerificationResultItem> Items { get; }

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="items">個別検証結果リスト</param>
		public VerificationResult(VerificationStatus status, IEnumerable<VerificationResultItem> items)
		{
			Source = items?.FirstOrDefault()?.Source;
			Status = status;
			Items = items;
		}
	}
}
