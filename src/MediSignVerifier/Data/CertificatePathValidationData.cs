using System.Collections.Generic;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// 証明書パス検証データ
	/// </summary>
	public class CertificatePathValidationData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="source">取得元、XMLではXPATHを設定します。</param>
		/// <param name="certificates">証明書群</param>
		/// <param name="crls">失効情報群(CRL)</param>
		/// <param name="ocsps">失効情報群(OCSP)</param>
		public CertificatePathValidationData(string source, IEnumerable<byte[]> certificates, IEnumerable<byte[]> crls, IEnumerable<byte[]> ocsps = null)
		{
			Source = source;
			Certificates = certificates;
			Crls = crls;
			Ocsps = ocsps;
		}

		/// <summary>
		/// 取得元
		/// </summary>
		public string Source { get; }

		/// <summary>
		/// 証明書群
		/// </summary>
		public IEnumerable<byte[]> Certificates { get; }

		/// <summary>
		/// 失効情報群(CRL)
		/// </summary>
		public IEnumerable<byte[]> Crls { get; }

		/// <summary>
		/// 失効情報群(OCSP)
		/// </summary>
		public IEnumerable<byte[]> Ocsps { get; }

	}
}
