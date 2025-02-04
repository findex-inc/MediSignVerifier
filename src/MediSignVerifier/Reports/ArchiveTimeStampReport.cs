using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// アーカイブタイムスタンプ
	/// </summary>
	public class ArchiveTimeStampReport : TimeStampReport
	{
		/// <summary>
		/// アーカイブタイムスタンプNo
		/// </summary>
		[JsonPropertyName("アーカイブタイムスタンプNo")]
		public int Index { get; private set; }

		/// <summary>
		/// <see cref="ArchiveTimeStampReport"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="index">アーカイブタイムスタンプNo</param>
		/// <param name="status">検証結果</param>
		/// <param name="id">Id</param>
		/// <param name="tsaCertificateBaseTime">TSA証明書検証基準時刻</param>
		/// <param name="timeStampGenTime">タイムスタンプ生成時刻</param>
		/// <param name="c14nAlgorithm">正規化アルゴリズム</param>
		/// <param name="digestAlgorithm">ダイジェストアルゴリズム</param>
		/// <param name="digestValue">ハッシュ値</param>
		/// <param name="calculatedValue">計算値</param>
		/// <param name="tsaSigInfo">TSAの署名情報</param>
		/// <param name="tsaCertInfo">TSA証明書情報</param>
		/// <param name="resultItems">検証結果詳細</param>
		/// <returns><see cref="ArchiveTimeStampReport"/> クラスの新しいインスタンス</returns>
		public static ArchiveTimeStampReport Create(
			int index,
			VerificationStatus status,
			string id,
			DateTime? tsaCertificateBaseTime,
			DateTime? timeStampGenTime,
			string c14nAlgorithm,
			string digestAlgorithm,
			string digestValue,
			string calculatedValue,
			TsaSignatureInfo tsaSigInfo,
			TsaCertificateInfo tsaCertInfo,
			IEnumerable<VerificationResultReportItem> resultItems
			)
		{
			return new ArchiveTimeStampReport
			{
				Index = index,
				Status = status,
				Id = id,
				TsaCertificateBaseTime = tsaCertificateBaseTime,
				TimeStampGenTime = timeStampGenTime,
				C14nAlgorithm = c14nAlgorithm,
				DigestAlgorithm = digestAlgorithm,
				DigestValue = digestValue,
				CalculatedValue = calculatedValue,
				TsaSignatureInfo = tsaSigInfo,
				TsaCertificateInfo = tsaCertInfo,
				ResultItems = resultItems.ToArray()
			};
		}
	}
}
