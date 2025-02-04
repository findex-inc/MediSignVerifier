using System.Text.Json.Serialization;
using SignatureVerifier.Data;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// TSA証明書情報
	/// </summary>
	public class TsaCertificateInfo
	{
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
		/// 証明書情報
		/// </summary>
		[JsonPropertyName("証明書情報")]
		public ICertificateInfo[] Certificates { get; private set; }

		/// <summary>
		/// <see cref="TsaCertificateInfo"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="digestAlgorithm">ダイジェストアルゴリズム</param>
		/// <param name="digestValue">ハッシュ値</param>
		/// <param name="calculatedValue">計算値</param>
		/// <param name="certs">証明書情報</param>
		/// <returns><see cref="TsaCertificateInfo"/> クラスの新しいインスタンス</returns>
		public static TsaCertificateInfo Create(
			string digestAlgorithm,
			string digestValue,
			string calculatedValue,
			ICertificateInfo[] certs
			)
		{
			return new TsaCertificateInfo
			{
				DigestAlgorithm = digestAlgorithm,
				DigestValue = digestValue,
				CalculatedValue = calculatedValue,
				Certificates = certs
			};
		}

		/// <summary>
		/// <see cref="TsaCertificateInfo"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="timeStampData">タイムスタンプ情報</param>
		/// <param name="certificates">証明書情報</param>
		/// <returns><see cref="TsaCertificateInfo"/> クラスの新しいインスタンス</returns>
		public static TsaCertificateInfo Create(TimeStampData timeStampData, ICertificateInfo[] certificates)
		{
			return Create(
				timeStampData.CertificateDigestAlgorithm,
				timeStampData.CertificateHashValue?.ToBase64String(),
				timeStampData.CertificateCalculatedValue?.ToBase64String(),
				certificates
				);
		}
	}
}
