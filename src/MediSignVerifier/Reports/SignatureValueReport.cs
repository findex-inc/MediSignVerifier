using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 署名データ
	/// </summary>
	public class SignatureValueReport
	{
		/// <summary>
		/// 検証結果
		/// </summary>
		[JsonPropertyName("検証結果")]
		public VerificationStatus Status { get; private set; }

		/// <summary>
		/// Id
		/// </summary>
		[JsonPropertyName("Id")]
		public string Id { get; private set; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		[JsonPropertyName("正規化アルゴリズム")]
		public string C14nAlgorithm { get; private set; }

		/// <summary>
		/// 署名アルゴリズム
		/// </summary>
		[JsonPropertyName("署名アルゴリズム")]
		public string SignatureAlgorithm { get; private set; }

		/// <summary>
		/// 検証結果詳細
		/// </summary>
		[JsonPropertyName("検証結果詳細")]
		public VerificationResultReportItem[] ResultItems { get; private set; }

		/// <summary>
		/// <see cref="SignatureValueReport" /> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="id">Id</param>
		/// <param name="c14nAlgorithm">正規化アルゴリズム</param>
		/// <param name="signatureAlgorithm">署名アルゴリズム</param>
		/// <param name="resultItems">検証結果詳細</param>
		/// <returns><see cref="SignatureValueReport" /> クラスの新しいインスタンス</returns>
		public static SignatureValueReport Create(
			VerificationStatus status,
			string id,
			string c14nAlgorithm,
			string signatureAlgorithm,
			IEnumerable<VerificationResultReportItem> resultItems
			)
		{
			return new SignatureValueReport
			{
				Status = status,
				Id = id,
				C14nAlgorithm = c14nAlgorithm,
				SignatureAlgorithm = signatureAlgorithm,
				ResultItems = resultItems.ToArray()
			};
		}
	}
}
