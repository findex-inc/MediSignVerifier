using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 署名構造
	/// </summary>
	public class StructureReport
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="resultItems">検証結果詳細</param>
		public StructureReport(VerificationStatus status, VerificationResultReportItem[] resultItems)
		{
			Status = status;
			ResultItems = resultItems;
		}

		/// <summary>
		/// 検証結果
		/// </summary>
		[JsonPropertyName("検証結果")]
		public VerificationStatus Status { get; }

		/// <summary>
		/// 検証結果詳細
		/// </summary>
		[JsonPropertyName("検証結果詳細")]
		public VerificationResultReportItem[] ResultItems { get; }
	}
}
