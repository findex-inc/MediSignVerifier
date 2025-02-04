namespace SignatureVerifier.Data
{
	/// <summary>
	/// 証明書データ
	/// </summary>
	public class CertificateData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="source">参照元、XMLではXPATHを設定します。</param>
		/// <param name="value">証明書</param>
		public CertificateData(string source, byte[] value)
			: this(source, null, value)
		{
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="source">取得元、XMLではXPATHを設定します。</param>
		/// <param name="id">参照元識別子、XMLではId属性を設定します。</param>
		/// <param name="value">証明書</param>
		public CertificateData(string source, string id, byte[] value)
		{
			Source = source;
			Id = id;
			Value = value;
		}

		/// <summary>
		/// 取得元
		/// </summary>
		public string Source { get; }

		/// <summary>
		/// 参照元識別子
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// 証明書
		/// </summary>
		public byte[] Value { get; }

	}
}
