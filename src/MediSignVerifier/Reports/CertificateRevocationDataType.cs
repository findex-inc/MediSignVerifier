namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 証明書の失効情報の種類を示します。
	/// </summary>
	public enum CertificateRevocationDataType
	{
		/// <summary>
		/// なし。
		/// </summary>
		None = 0,

		/// <summary>
		/// CRL.
		/// </summary>
		CRL,

		/// <summary>
		/// OCSP.
		/// </summary>
		OCSP
	}

}
