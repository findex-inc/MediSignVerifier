using System;
using System.Text.Json.Serialization;
using SignatureVerifier.Reports;

namespace SignatureVerifier
{
	/// <summary>
	/// 署名検証結果レポート
	/// </summary>
	public class SignatureVerificationReport
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="targetFileName">検証ファイル名</param>
		/// <param name="creationTime">レポート作成時刻</param>
		/// <param name="verificationTime">検証時刻</param>
		/// <param name="status">検証結果</param>
		/// <param name="documentType">電子処方箋種別</param>
		public SignatureVerificationReport(
			string targetFileName,
			DateTime creationTime,
			DateTime verificationTime,
			VerificationStatus status,
			DocumentType documentType)
		{
			TargetFileName = targetFileName;
			CreationTime = creationTime;
			VerificationTime = verificationTime;
			Status = status;
			DocumentType = documentType;
		}

		/// <summary>
		/// 検証ファイル名
		/// </summary>
		[JsonPropertyName("検証ファイル名")]
		public string TargetFileName { get; }

		/// <summary>
		/// レポート作成時刻
		/// </summary>
		[JsonPropertyName("レポート作成時刻")]
		public DateTime CreationTime { get; }

		/// <summary>
		/// 検証時刻
		/// </summary>
		[JsonPropertyName("検証時刻")]
		public DateTime VerificationTime { get; }

		/// <summary>
		/// 検証結果
		/// </summary>
		[JsonPropertyName("検証結果")]
		public VerificationStatus Status { get; }

		/// <summary>
		/// 検証結果メッセージ
		/// </summary>
		[JsonPropertyName("検証結果メッセージ")]
		public string Message { get; internal set; }

		/// <summary>
		/// 電子処方箋種別
		/// </summary>
		[JsonPropertyName("電子処方箋種別")]
		public DocumentType DocumentType { get; }

		/// <summary>
		/// 署名構造
		/// </summary>
		[JsonPropertyName("署名構造")]
		public StructureReport Structure { get; internal set; }

		/// <summary>
		/// 署名リスト
		/// </summary>
		[JsonPropertyName("署名リスト")]
		public SignatureReportSet[] Signatures { get; internal set; }


	}
}
