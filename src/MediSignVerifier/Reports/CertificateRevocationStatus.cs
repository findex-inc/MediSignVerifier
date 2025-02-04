namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 証明書の失効状態を示します。
	/// </summary>
	public enum CertificateRevocationStatus
	{
		/// <summary>
		/// 失効していません。
		/// </summary>
		UNREVOKED = 0,

		/// <summary>
		/// 失効しています。
		/// </summary>
		REVOKED,

		/// <summary>
		/// 失効ステータスがまだ決定されていません。
		/// </summary>
		UNDETERMINED,
	}
}
