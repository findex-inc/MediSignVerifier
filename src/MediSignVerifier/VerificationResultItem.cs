
namespace SignatureVerifier
{
	/// <summary>
	/// 検証項目ごとの検証結果
	/// </summary>
	public class VerificationResultItem
	{
		/// <summary>検証結果</summary>
		public VerificationStatus Status { get; }

		/// <summary>署名元データの種別</summary>
		public SignatureSourceType Type { get; }

		/// <summary>検証対象</summary>
		public string Source { get; }

		/// <summary>インデックス(同一要素が複数ある場合)</summary>
		public int Index { get; }

		/// <summary>検証要件</summary>
		public string ItemName { get; }

		/// <summary>元データの項目を特定する情報</summary>
		public string MappedItem { get; }

		/// <summary>メッセージ</summary>
		public string Message { get; }


		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="type">署名元データの種別</param>
		/// <param name="source">検証対象</param>
		/// <param name="itemName">検証要件</param>
		/// <param name="mappedItem">元データの項目を特定する情報</param>
		/// <param name="message">メッセージ</param>
		public VerificationResultItem(VerificationStatus status, SignatureSourceType type, string source, string itemName, string mappedItem, string message)
		{
			Status = status;
			Type = type;
			Source = source;
			ItemName = itemName;
			MappedItem = mappedItem;
			Message = message;

			Index = -1;
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="type">署名元データの種別</param>
		/// <param name="source">検証対象</param>
		/// <param name="index">インデックス</param>
		/// <param name="itemName">検証要件</param>
		/// <param name="mappedItem">元データの項目を特定する情報</param>
		/// <param name="message">メッセージ</param>
		public VerificationResultItem(VerificationStatus status, SignatureSourceType type, string source, int index, string itemName, string mappedItem, string message)
			: this(status, type, source, itemName, mappedItem, message)
		{
			Index = index;
		}
	}
}
