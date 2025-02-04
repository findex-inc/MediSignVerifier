using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 参照データ
	/// </summary>
	public class ReferenceReport
	{
		/// <summary>
		/// 参照データNo
		/// </summary>
		[JsonPropertyName("参照データNo")]
		public int Index { get; private set; }

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
		/// 参照先
		/// </summary>
		[JsonPropertyName("参照先")]
		public string Uri { get; private set; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		[JsonPropertyName("正規化アルゴリズム")]
		public string C14nAlgorithm { get; private set; }

		/// <summary>
		/// ダイジェストアルゴリズム
		/// </summary>
		[JsonPropertyName("ダイジェストアルゴリズム")]
		public string DigestAlgorithm { get; private set; }

		/// <summary>
		/// ハッシュ値
		/// </summary>
		[JsonPropertyName("ハッシュ値")]
		public string DigestValue { get; private set; }

		/// <summary>
		/// 計算値
		/// </summary>
		[JsonIgnore]
		public string CalculatedValue { get; private set; }

		/// <summary>
		/// 計算値
		/// </summary>
		[JsonPropertyName("計算値")]
		public string CalculatedDispValue
		{
			get
			{
				//同値の場合は出力しない
				return DigestValue != CalculatedValue ? CalculatedValue : null;
			}
		}

		/// <summary>
		/// 検証結果詳細
		/// </summary>
		[JsonPropertyName("検証結果詳細")]
		public VerificationResultReportItem[] ResultItems { get; private set; }

		/// <summary>
		/// <see cref="ReferenceReport"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="index">参照データNo</param>
		/// <param name="status">検証結果</param>
		/// <param name="id">Id</param>
		/// <param name="uri">参照先</param>
		/// <param name="c14nAlgorithm">正規化アルゴリズム</param>
		/// <param name="digestAlgorithm">ダイジェストアルゴリズム</param>
		/// <param name="digestValue">ハッシュ値</param>
		/// <param name="calculatedValue">計算値</param>
		/// <param name="resultItems">検証結果詳細</param>
		/// <returns><see cref="ReferenceReport"/> クラスの新しいインスタンス</returns>
		public static ReferenceReport Create(
			int index,
			VerificationStatus status,
			string id,
			string uri,
			string c14nAlgorithm,
			string digestAlgorithm,
			string digestValue,
			string calculatedValue,
			IEnumerable<VerificationResultReportItem> resultItems
			)
		{
			return new ReferenceReport
			{
				Index = index,
				Status = status,
				Id = id,
				Uri = uri,
				C14nAlgorithm = c14nAlgorithm,
				DigestAlgorithm = digestAlgorithm,
				DigestValue = digestValue,
				CalculatedValue = calculatedValue,
				ResultItems = resultItems.ToArray()
			};
		}
	}
}
