using System.Text.Json.Serialization;
using SignatureVerifier.Data;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// TSA署名情報
	/// </summary>
	public class TsaSignatureInfo
	{
		/// <summary>
		/// ダイジェストアルゴリズム
		/// </summary>
		[JsonPropertyName("ダイジェストアルゴリズム")]
		public string DigestAlgorithm { get; private set; }

		/// <summary>
		/// 署名アルゴリズム
		/// </summary>
		[JsonPropertyName("署名アルゴリズム")]
		public string SignatureAlgorithm { get; private set; }

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
		/// <see cref="TsaSignatureInfo"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="digestAlgorithm">ダイジェストアルゴリズム</param>
		/// <param name="signatureAlgorithm">署名アルゴリズム</param>
		/// <param name="digestValue">ハッシュ値</param>
		/// <param name="calculatedValue">計算値</param>
		/// <returns><see cref="TsaSignatureInfo"/> クラスの新しいインスタンス</returns>
		public static TsaSignatureInfo Create(
			string digestAlgorithm,
			string signatureAlgorithm,
			string digestValue,
			string calculatedValue
			)
		{
			return new TsaSignatureInfo
			{
				DigestAlgorithm = digestAlgorithm,
				SignatureAlgorithm = signatureAlgorithm,
				DigestValue = digestValue,
				CalculatedValue = calculatedValue
			};
		}

		/// <summary>
		/// <see cref="TsaSignatureInfo"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="timeStampData">タイムスタンプ情報</param>
		/// <returns><see cref="TsaSignatureInfo"/> クラスの新しいインスタンス</returns>
		public static TsaSignatureInfo Create(TimeStampData timeStampData)
		{
			return Create(
				timeStampData.SignerDigestAlgorithm,
				timeStampData.SignerSignatureAlgorithm,
				timeStampData.MessageDigestValue?.ToBase64String(),
				timeStampData.MessageCalculatedValue?.ToBase64String()
				);
		}
	}
}
