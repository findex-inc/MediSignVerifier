using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 検証結果詳細
	/// </summary>
	public class VerificationResultReportItem
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="itemName">検証内容</param>
		/// <param name="status">結果</param>
		/// <param name="message">メッセージ</param>
		public VerificationResultReportItem(string itemName, VerificationStatus status, string message)
		{
			ItemName = itemName;
			Status = status;
			Message = message;
		}

		/// <summary>
		/// 結果
		/// </summary>
		[JsonPropertyName("結果")]
		public VerificationStatus Status { get; }

		/// <summary>
		/// 検証内容
		/// </summary>
		[JsonPropertyName("検証内容")]
		public string ItemName { get; }

		/// <summary>
		/// メッセージ
		/// </summary>
		[JsonPropertyName("メッセージ")]
		public string Message { get; }
	}
}
