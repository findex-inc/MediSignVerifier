using System;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 証明書情報を示します。
	/// </summary>
	public interface ICertificateInfo
	{
		/// <summary>
		/// 状態
		/// </summary>
		[JsonPropertyName("状態")]
		CertificateRevocationStatus CertStatus { get; }

		/// <summary>
		/// 取得元
		/// </summary>
		[JsonPropertyName("取得元")]
		string Source { get; }

		/// <summary>
		/// シリアル番号
		/// </summary>
		[JsonPropertyName("シリアル番号")]
		string SerialNumber { get; }

		/// <summary>
		/// 発行者
		/// </summary>
		[JsonPropertyName("発行者")]
		string Issuer { get; }

		/// <summary>
		/// サブジェクト
		/// </summary>
		[JsonPropertyName("サブジェクト")]
		string Subject { get; }

		/// <summary>
		/// 有効期間の開始
		/// </summary>
		[JsonPropertyName("有効期間の開始")]
		DateTime? NotBefore { get; }

		/// <summary>
		/// 有効期間の終了
		/// </summary>
		[JsonPropertyName("有効期間の終了")]
		DateTime? NotAfter { get; }

		/// <summary>
		/// isCa
		/// </summary>
		[JsonPropertyName("isCa")]
		bool? IsCa { get; }

		/// <summary>
		/// hcRole
		/// </summary>
		[JsonPropertyName("hcRole")]
		string HcRole { get; }

		/// <summary>
		/// 拡張キー使用法
		/// </summary>
		[JsonPropertyName("拡張キー使用法")]
		string[] ExtendedKeyUsage { get; }

		/// <summary>
		/// 失効情報
		/// </summary>
		[JsonPropertyName("失効情報")]
		ICertificateRevocationInfo[] Revocations { get; }

	}
}
