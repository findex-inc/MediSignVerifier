using System;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// タイムスタンプ情報
	/// </summary>
	public class TimeStampData
	{
		/// <summary>
		/// 検証基準時刻
		/// </summary>
		public DateTime ValidDate { get; internal set; }


		/// <summary>
		/// タイムスタンプ生成時刻
		/// </summary>
		public DateTime? GenTime { get; internal set; }

		/// <summary>
		/// MessageImprintの hashAlgorithm
		/// </summary>
		public string HashAlgorithm { get; internal set; }

		/// <summary>
		/// MessageImprintの hashMessage
		/// </summary>
		public byte[] HashValue { get; internal set; }

		/// <summary>
		/// 計算値：タイムスタンプ対象データのハッシュ
		/// </summary>
		public byte[] CalculatedValue { get; internal set; }

		/// <summary>
		/// signerInfoのdigestAlgorithm
		/// </summary>
		public string SignerDigestAlgorithm { get; internal set; }

		/// <summary>
		/// signerInfoのsignatureAlgorithm
		/// </summary>
		public string SignerSignatureAlgorithm { get; internal set; }

		/// <summary>
		/// signerInfo.signedAttrsのMessageDigest
		/// </summary>
		public byte[] MessageDigestValue { get; internal set; }

		/// <summary>
		/// 計算値：signerInfoのeContentのハッシュ
		/// </summary>
		public byte[] MessageCalculatedValue { get; internal set; }

		/// <summary>
		/// SigningCertificateに含まれる ダイジェストアルゴリズム
		/// </summary>
		public string CertificateDigestAlgorithm { get; internal set; }

		/// <summary>
		/// SigningCertificateに含まれるハッシュ値
		/// </summary>
		public byte[] CertificateHashValue { get; internal set; }

		/// <summary>
		/// 計算値：TSA証明書のハッシュ
		/// </summary>
		public byte[] CertificateCalculatedValue { get; internal set; }
	}
}
