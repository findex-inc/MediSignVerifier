using System;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// 変換時のエラーが発生した場合にスローされる例外。
	/// </summary>
	public class TimeStampConvertException : Exception
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="itemName">この例外が発生した項目名。</param>
		/// <param name="message">この例外の原因を説明するエラー メッセージ。</param>
		/// <param name="innerException">現在の例外の原因である例外。内部例外が指定されていない場合は null 参照。</param>
		public TimeStampConvertException(string itemName, string message, Exception innerException = null)
			: base(message, innerException)
		{
			ItemName = itemName;
		}

		/// <summary>
		/// この例外が発生した項目名を取得します。
		/// </summary>
		public string ItemName { get; }
	}
}
