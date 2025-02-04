using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 署名者証明書
	/// </summary>
	public class SigningCertificateReport
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
		/// 証明書検証基準時刻
		/// </summary>
		[JsonPropertyName("証明書検証基準時刻")]
		public DateTime CertificateBaseTime { get; private set; }

		//[JsonPropertyName("失効情報検証基準時刻")]
		//public DateTime RevocationBaseTime { get; private set; }

		/// <summary>
		/// 証明書情報
		/// </summary>
		[JsonPropertyName("証明書情報")]
		public ICertificateInfo[] Certificates { get; private set; }

		/// <summary>
		/// 検証結果詳細
		/// </summary>
		[JsonPropertyName("検証結果詳細")]
		public VerificationResultReportItem[] ResultItems { get; private set; }

		/// <summary>
		/// <see cref="SigningCertificateReport"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="id">Id</param>
		/// <param name="certificateBaseTime">証明書検証基準時刻</param>
		/// <param name="certificates">証明書情報</param>
		/// <param name="resultItems">検証結果詳細</param>
		/// <returns><see cref="SigningCertificateReport"/> クラスの新しいインスタンス</returns>
		public static SigningCertificateReport Create(
			VerificationStatus status,
			string id,
			DateTime certificateBaseTime,
			IEnumerable<ICertificateInfo> certificates,
			IEnumerable<VerificationResultReportItem> resultItems)
		{
			return new SigningCertificateReport
			{
				Status = status,
				Id = id,
				CertificateBaseTime = certificateBaseTime,
				Certificates = certificates?.ToArray(),
				ResultItems = resultItems?.ToArray(),
			};
		}

	}
}
