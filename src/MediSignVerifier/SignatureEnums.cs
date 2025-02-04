
namespace SignatureVerifier
{
	/// <summary>
	/// XAdESレベル
	/// </summary>
	public enum ESLevel
	{
		/// <summary>なし</summary>
		None = 0,

		/// <summary>XAdES-BES</summary>
		BES,

		/// <summary>XAdES-T</summary>
		T,

		/// <summary>XAdES-XL</summary>
		XL,

		/// <summary>XAdES-A</summary>
		A
	}

	/// <summary>
	/// 処方箋種別
	/// </summary>
	public enum DocumentType
	{
		/// <summary>不明</summary>
		Unknown = 0,

		/// <summary>処方</summary>
		Prescription,

		/// <summary>調剤</summary>
		Dispensing,
	}

	/// <summary>
	/// 署名元データ種別
	/// </summary>
	public enum SignatureSourceType
	{
		/// <summary>なし</summary>
		None = 0,

		/// <summary>処方</summary>
		Prescription,

		/// <summary>調剤</summary>
		Dispensing,

		/// <summary>調剤内処方</summary>
		DispPrescription,

		/// <summary>不明</summary>
		Unknown,
	}

	/// <summary>
	/// 検証結果
	/// </summary>
	public enum VerificationStatus
	{
		/// <summary>有効</summary>
		VALID = 0,

		/// <summary>無効</summary>
		INVALID,

		/// <summary>未確定</summary>
		INDETERMINATE
	}

	/// <summary>
	/// タイムスタンプ検証内容[処理続行可能なもののみ]
	/// </summary>
	internal static class TimeStampItemNames
	{
		public static readonly string Target = "タイムスタンプ対象データ";
		public static readonly string Token = "タイムスタンプトークン";
		public static readonly string TSACert = "TSA証明書";
		public static readonly string TSASignature = "TSAの署名";
		public static readonly string MessageImprint = "MessageImprint値";
	}
}
