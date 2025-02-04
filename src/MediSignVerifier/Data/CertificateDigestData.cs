namespace SignatureVerifier.Data
{
	/// <summary>
	/// 証明書ダイジェストデータ
	/// </summary>
	public class CertificateDigestData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">参照元識別子、XMLではId属性を設定します。</param>
		/// <param name="method">ダイジェストメソッド、XMLではURI形式の<c>Algorithm</c>属性を設定します。</param>
		/// <param name="value">証明書</param>
		public CertificateDigestData(string id, string method, byte[] value)
		{
			Id = id;
			DigestMethod = method;
			DigestValue = value;
		}

		/// <summary>
		/// 参照元識別子
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// ダイジェストメソッド
		/// </summary>
		public string DigestMethod { get; }

		/// <summary>
		/// ダイジェスト値
		/// </summary>
		public byte[] DigestValue { get; }
	}

}
