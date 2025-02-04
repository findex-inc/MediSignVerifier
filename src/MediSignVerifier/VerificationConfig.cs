namespace SignatureVerifier
{
	/// <summary>
	/// 検証オプション
	/// </summary>
	public class VerificationConfig
	{
		/// <summary>
		/// HPKI固有の検証を有効にするかどうかを示す値を取得または設定します。
		/// </summary>
		public bool HPKIValidationEnabled { get; set; } = true;
	}
}
