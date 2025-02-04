namespace SignatureVerifier.Data
{
	/// <summary>
	/// 証明書発行者データ
	/// </summary>
	public class CertificateIssuerSerialData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">参照元識別子、XMLではId属性を設定します。</param>
		/// <param name="issuer">発行者名</param>
		/// <param name="serial">シリアルナンバー</param>
		public CertificateIssuerSerialData(string id, string issuer, string serial)
		{
			Id = id;
			IssuerName = issuer;
			SerialNumber = serial;
		}

		/// <summary>
		/// 参照元識別子
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// 発行者名
		/// </summary>
		public string IssuerName { get; }

		/// <summary>
		/// シリアルナンバー
		/// </summary>
		public string SerialNumber { get; }

	}
}
