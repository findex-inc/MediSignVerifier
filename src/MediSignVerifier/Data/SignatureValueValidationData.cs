using System;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// 署名値検証データ
	/// </summary>
	public class SignatureValueValidationData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">SignatureValue要素のID</param>
		/// <param name="canonicalizationMethod">正規化アルゴリズム</param>
		/// <param name="signatureMethod">署名アルゴリズム</param>
		/// <param name="signatureValue">署名値</param>
		/// <param name="targetValue">署名対象値</param>
		public SignatureValueValidationData(string id, string canonicalizationMethod, string signatureMethod, byte[] signatureValue, byte[] targetValue)
		{
			Id = id;
			CanonicalizationMethod = canonicalizationMethod;
			SignatureMethod = signatureMethod;
			SignatureValue = signatureValue;
			TargetValue = targetValue;
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">SignatureValue要素のID</param>
		/// <param name="canonicalizationMethod">正規化アルゴリズム</param>
		/// <param name="signatureMethod">署名アルゴリズム</param>
		/// <param name="signatureValue">署名値</param>
		/// <param name="targetValue">署名対象値</param>
		/// <param name="convertError">変換エラー</param>
		public SignatureValueValidationData(string id, string canonicalizationMethod, string signatureMethod, byte[] signatureValue, byte[] targetValue, Exception convertError)
			: this(id, canonicalizationMethod, signatureMethod, signatureValue, targetValue)
		{
			ConvertError = convertError;
		}

		/// <summary>
		/// SignatureValue要素のID
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		public string CanonicalizationMethod { get; }

		/// <summary>
		/// 署名アルゴリズム
		/// </summary>
		public string SignatureMethod { get; }

		/// <summary>
		/// 署名値
		/// </summary>
		public byte[] SignatureValue { get; }

		/// <summary>
		/// 署名対象値
		/// </summary>
		public byte[] TargetValue { get; }

		/// <summary>
		/// 変換エラー
		/// </summary>
		public Exception ConvertError { get; }
	}
}
