using System;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// Reference検証データ
	/// </summary>
	public class ReferenceValidationData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">Reference要素のID</param>
		/// <param name="index">Reference要素の順番</param>
		/// <param name="uri">参照先URI</param>
		/// <param name="transform">正規化アルゴリズム</param>
		/// <param name="digestMethod">ダイジェストアルゴリズム</param>
		/// <param name="digestValue">XML内のハッシュ値</param>
		/// <param name="calculatedValue">計算したハッシュ値</param>
		public ReferenceValidationData(string id, int index, string uri, string transform, string digestMethod, byte[] digestValue, byte[] calculatedValue)
		{
			Id = id;
			Index = index;
			Uri = uri;
			Transform = transform;
			DigestMethod = digestMethod;
			DigestValue = digestValue;
			CalculatedValue = calculatedValue;
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">Reference要素のID</param>
		/// <param name="index">Reference要素の順番</param>
		/// <param name="uri">参照先URI</param>
		/// <param name="transform">正規化アルゴリズム</param>
		/// <param name="digestMethod">ダイジェストアルゴリズム</param>
		/// <param name="digestValue">XML内のハッシュ値</param>
		/// <param name="calculatedValue">計算したハッシュ値</param>
		/// <param name="convertError">変換エラー</param>
		public ReferenceValidationData(string id, int index, string uri, string transform, string digestMethod, byte[] digestValue, byte[] calculatedValue, Exception convertError)
			: this(id, index, uri, transform, digestMethod, digestValue, calculatedValue)
		{
			ConvertError = convertError;
		}

		/// <summary>
		/// Reference要素のID
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Reference要素の順番
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// Reference要素の表示用Index（Indexに+1したもの）
		/// </summary>
		public int DispIndex => Index + 1;

		/// <summary>
		/// 参照先URI
		/// </summary>
		public string Uri { get; }

		/// <summary>
		/// 正規化アルゴリズム
		/// </summary>
		public string Transform { get; }

		/// <summary>
		/// ダイジェストアルゴリズム
		/// </summary>
		public string DigestMethod { get; }

		/// <summary>
		/// XML内のハッシュ値
		/// </summary>
		public byte[] DigestValue { get; }

		/// <summary>
		/// 計算したハッシュ値
		/// </summary>
		public byte[] CalculatedValue { get; }

		/// <summary>
		/// 変換エラー
		/// </summary>
		public Exception ConvertError { get; }

	}
}
