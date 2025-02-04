namespace SignatureVerifier.Data
{
	/// <summary>
	/// アーカイブタイムスタンプ検証データ
	/// </summary>
	public class ArchiveTimeStampValidationData
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="id">ArchiveTimeStamp要素のID</param>
		/// <param name="index">タイムスタンプのIndex</param>
		/// <param name="validationData">タイムスタンプ検証データ</param>
		/// <param name="convertError">変換エラー</param>
		public ArchiveTimeStampValidationData(string id, int index, TimeStampValidationData validationData, TimeStampConvertException convertError)
		{
			Id = id;
			Index = index;
			ValidationData = validationData;
			ConvertError = convertError;
		}

		/// <summary>
		/// ArchiveTimeStamp要素のID
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// タイムスタンプのIndex（ATS用）
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// タイムスタンプの表示用Index（Indexに+1したもの）
		/// </summary>
		public int DispIndex => Index + 1;

		/// <summary>
		/// タイムスタンプ検証データ
		/// </summary>
		public TimeStampValidationData ValidationData { get; }

		/// <summary>
		/// 変換エラー
		/// </summary>
		public TimeStampConvertException ConvertError { get; }
	}
}
