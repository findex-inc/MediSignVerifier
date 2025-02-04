using System;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 証明書失効情報を示します。
	/// </summary>
	public interface ICertificateRevocationInfo
	{
		/// <summary>
		/// 種類
		/// </summary>
		[JsonPropertyName("種類")]
		CertificateRevocationDataType DataType { get; }

		/// <summary>
		/// 取得元
		/// </summary>
		[JsonPropertyName("取得元")]
		string Source { get; }

		/// <summary>
		/// 発行者
		/// </summary>
		[JsonPropertyName("発行者")]
		string Issuer { get; }

		/// <summary>
		/// 有効開始日
		/// </summary>
		[JsonPropertyName("有効開始日")]
		DateTime? ThisUpdate { get; }

		/// <summary>
		/// 次の更新予定
		/// </summary>
		[JsonPropertyName("次の更新予定")]
		DateTime? NextUpdate { get; }

		/// <summary>
		/// 失効リスト
		/// </summary>
		[JsonPropertyName("失効リスト")]
		ICertificateRevocationInfoEntry[] Entries { get; }

	}

	/// <summary>
	/// 証明書失効情報失効リストを示します。
	/// </summary>
	public interface ICertificateRevocationInfoEntry
	{
		/// <summary>
		/// シリアル番号
		/// </summary>
		[JsonPropertyName("シリアル番号")]
		string SerialNumber { get; }

		/// <summary>
		/// 失効日
		/// </summary>
		[JsonPropertyName("失効日")]
		DateTime? RevocationDate { get; }

		/// <summary>
		/// 理由
		/// </summary>
		[JsonPropertyName("理由")]
		CertificateRevocationReason Reason { get; }

	}
}
