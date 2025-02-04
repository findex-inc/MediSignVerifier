using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 署名レポート一式
	/// </summary>
	public class SignatureReportSet
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="index">署名No</param>
		/// <param name="sourceType">署名種別</param>
		/// <param name="esLevel">署名フォーマット</param>
		/// <param name="path">パス</param>
		public SignatureReportSet(int index, SignatureSourceType sourceType, ESLevel esLevel, string path)
		{
			Index = index;
			SourceType = sourceType;
			ESLevel = esLevel;
			Path = path;
		}

		/// <summary>
		/// 署名No
		/// </summary>
		[JsonPropertyName("署名No")]
		public int Index { get; }

		/// <summary>
		/// 署名種別
		/// </summary>
		[JsonPropertyName("署名種別")]
		public SignatureSourceType SourceType { get; }

		/// <summary>
		/// 署名フォーマット
		/// </summary>
		[JsonPropertyName("署名フォーマット")]
		public ESLevel ESLevel { get; }

		/// <summary>
		/// パス
		/// </summary>
		[JsonPropertyName("パス")]
		public string Path { get; }

		/// <summary>
		/// 署名構造
		/// </summary>
		[JsonPropertyName("署名構造")]
		public StructureReport Structure { get; internal set; }

		/// <summary>
		/// 署名者証明書
		/// </summary>
		[JsonPropertyName("署名者証明書")]
		public SigningCertificateReport SigningCertificate { get; internal set; }

		/// <summary>
		/// 参照データ
		/// </summary>
		[JsonPropertyName("参照データ")]
		public ReferenceReportList References { get; internal set; }

		/// <summary>
		/// 署名データ
		/// </summary>
		[JsonPropertyName("署名データ")]
		public SignatureValueReport SignatureValue { get; internal set; }

		/// <summary>
		/// 署名タイムスタンプ
		/// </summary>
		[JsonPropertyName("署名タイムスタンプ")]
		public SignatureTimeStampReport SignatureTimeStamp { get; internal set; }

		/// <summary>
		/// アーカイブタイムスタンプ
		/// </summary>
		[JsonPropertyName("アーカイブタイムスタンプ")]
		public ArchiveTimeStampReportList ArchiveTimeStamps { get; internal set; }
	}
}
