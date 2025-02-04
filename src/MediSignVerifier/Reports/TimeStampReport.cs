using System;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// タイムスタンプ検証結果で共通のプロパティ
	/// </summary>
	public class TimeStampReport
	{
		/// <summary>
		/// 検証結果
		/// </summary>
		[JsonPropertyName("検証結果")]
		public VerificationStatus Status { get; protected set; }

		/// <summary>
		///  Id
		/// </summary>
		[JsonPropertyName("Id")]
		public string Id { get; protected set; }

		/// <summary>
		/// TSA証明書検証基準時刻
		/// </summary>
		[JsonPropertyName("TSA証明書検証基準時刻")]
		public DateTime? TsaCertificateBaseTime { get; protected set; }

		/// <summary>
		/// タイムスタンプ生成時刻
		/// </summary>
		[JsonPropertyName("タイムスタンプ生成時刻")]
		public DateTime? TimeStampGenTime { get; protected set; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		[JsonPropertyName("正規化アルゴリズム")]
		public string C14nAlgorithm { get; protected set; }

		/// <summary>
		/// ダイジェストアルゴリズム
		/// </summary>
		[JsonPropertyName("ダイジェストアルゴリズム")]
		public string DigestAlgorithm { get; protected set; }

		/// <summary>
		/// ハッシュ値
		/// </summary>
		[JsonPropertyName("ハッシュ値")]
		public string DigestValue { get; protected set; }

		/// <summary>
		/// 計算値
		/// </summary>
		[JsonIgnore]
		public string CalculatedValue { get; protected set; }

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
		/// TSAの署名情報
		/// </summary>
		[JsonPropertyName("TSAの署名情報")]
		public TsaSignatureInfo TsaSignatureInfo { get; protected set; }

		/// <summary>
		/// TSA証明書情報
		/// </summary>
		[JsonPropertyName("TSA証明書情報")]
		public TsaCertificateInfo TsaCertificateInfo { get; protected set; }

		/// <summary>
		/// 検証結果詳細
		/// </summary>
		[JsonPropertyName("検証結果詳細")]
		public VerificationResultReportItem[] ResultItems { get; protected set; }

	}
}
