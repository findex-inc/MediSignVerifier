using System.Collections.Generic;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// タイムスタンプ検証データ
	/// </summary>
	public class TimeStampValidationData
	{
		private TimeStampData _tsData;

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">TimeStamp要素のID</param>
		/// <param name="timeStamp">タイムスタンプ</param>
		/// <param name="c14nMethod">正規化アルゴリズム</param>
		/// <param name="targetValue">タイムスタンプ対象値</param>
		/// <param name="tsaCertificate">TSA証明書データ</param>
		/// <param name="certificatePathValidationData">TSA証明書検証データ</param>
		/// <param name="certErrors">証明書or失効リスト変換エラー</param>
		public TimeStampValidationData(string id, byte[] timeStamp,
			string c14nMethod,
			byte[] targetValue,
			CertificateData tsaCertificate,
			CertificatePathValidationData certificatePathValidationData,
			IEnumerable<TimeStampConvertException> certErrors)
		{
			Id = id;
			TimeStamp = timeStamp;
			CanonicalizationMethod = c14nMethod;
			TargetValue = targetValue;
			TsaCertificate = tsaCertificate;
			CertificateValidationData = certificatePathValidationData;
			CertErrors = certErrors;
		}


		/// <summary>
		/// TimeStamp要素のID
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// タイムスタンプ
		/// </summary>
		public byte[] TimeStamp { get; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		public string CanonicalizationMethod { get; }

		/// <summary>
		/// タイムスタンプ対象値
		/// </summary>
		public byte[] TargetValue { get; }

		/// <summary>
		/// TSA証明書データ
		/// </summary>
		public CertificateData TsaCertificate { get; }

		/// <summary>
		/// TSA証明書検証データ
		/// </summary>
		public CertificatePathValidationData CertificateValidationData { get; }

		/// <summary>
		/// タイムスタンプ解析情報
		/// </summary>
		public TimeStampData TimeStampData => _tsData;

		/// <summary>
		/// 証明書or失効リスト変換エラー
		/// </summary>
		internal IEnumerable<TimeStampConvertException> CertErrors { get; }

		/// <summary>
		/// 解析情報のセット
		/// </summary>
		/// <param name="data">タイムスタンプデータ</param>
		internal void SetTimeStampData(TimeStampData data)
		{
			_tsData = data;
		}
	}
}
