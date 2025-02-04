using SignatureVerifier.Data;

namespace SignatureVerifier
{
	/// <summary>
	/// 署名者証明書検証用データ
	/// </summary>
	public interface ISigningCertificateValidationData
	{
		/// <summary>
		/// 参照先識別
		/// </summary>
		/// <remarks>
		/// <p>XAdESの場合には "KeyInfo" / "SigningCertificate" / "SigningCertificateV2" </p>.
		/// </remarks>
		string Referenced { get; }

		/// <summary>
		/// 証明書
		/// </summary>
		/// <remarks>
		/// <para><c>KeyInfo/X509Certificate</c>の場合にはその値を設定します。
		/// <c>KeyInfo/X509IssuerSerial</c>や<c>SigningCertificate(V2)</c>の場合には、
		/// <c>CertificateValues</c>から一致する証明書を探して取得します。
		/// 無ければ<c>null</c>になります。</para>
		/// </remarks>
		CertificateData Certificate { get; }

		/// <summary>
		/// 証明書ダイジェスト
		/// </summary>
		/// <remarks>
		/// <para><c>SigningCertificate(V2)</c>の場合に取得します。
		/// 上記以外の場合には<c>null</c>になります。</para>
		/// </remarks>
		CertificateDigestData CertDigest { get; }

		/// <summary>
		/// 証明書発行者
		/// </summary>
		/// <remarks>
		/// <para><c>KeyInfo/X509IssuerSerial</c>や<c>SigningCertificate(V2)</c>の場合に取得します。
		/// 上記以外の場合には<c>null</c>になります。</para>
		/// </remarks>
		CertificateIssuerSerialData IssuerSerial { get; }

		/// <summary>
		/// 署名者証明書パス検証データ
		/// </summary>
		CertificatePathValidationData PathValidationData { get; }

	}
}

