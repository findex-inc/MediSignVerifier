using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// アーカイブタイムスタンプリスト
	/// </summary>
	public class ArchiveTimeStampReportList
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
		public ArchiveTimeStampReport[] Items { get; private set; }

		/// <summary>
		/// <see cref="ArchiveTimeStampReportList"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="message">メッセージ</param>
		/// <param name="items">リスト</param>
		/// <returns><see cref="ArchiveTimeStampReportList"/> クラスの新しいインスタンス</returns>
		public static ArchiveTimeStampReportList Create(
			VerificationStatus status,
			string message,
			IEnumerable<ArchiveTimeStampReport> items)
		{
			return new ArchiveTimeStampReportList
			{
				Status = status,
				Message = message,
				Items = items.ToArray()
			};
		}
	}
}
