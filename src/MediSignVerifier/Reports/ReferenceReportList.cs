using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 参照データリスト
	/// </summary>
	public class ReferenceReportList
	{
		/// <summary>
		/// 検証結果
		/// </summary>
		[JsonPropertyName("検証結果")]
		public VerificationStatus Status { get; private set; }

		/// <summary>
		/// メッセージ
		/// </summary>
		[JsonPropertyName("メッセージ")]
		public string Message { get; private set; }

		/// <summary>
		/// リスト
		/// </summary>
		[JsonPropertyName("リスト")]
		public ReferenceReport[] Items { get; private set; }

		/// <summary>
		/// <see cref="ReferenceReportList"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="message">メッセージ</param>
		/// <param name="items">リスト</param>
		/// <returns><see cref="ReferenceReportList"/> クラスの新しいインスタンス</returns>
		public static ReferenceReportList Create(
			VerificationStatus status,
			string message,
			IEnumerable<ReferenceReport> items)
		{
			return new ReferenceReportList
			{
				Status = status,
				Message = message,
				Items = items.ToArray()
			};
		}
	}
}
