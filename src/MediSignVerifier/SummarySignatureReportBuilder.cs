using SignatureVerifier.Reports.SummaryBuilders;

namespace SignatureVerifier
{
	/// <summary>
	/// 署名検証結果サマリレポート生成クラス
	/// </summary>
	public abstract class SummarySignatureReportBuilder
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="original">検証結果レポート（JSON）を指定します。</param>
		/// <param name="config">サマリレポート作成で使用するオプションを指定します。</param>
		protected SummarySignatureReportBuilder(string original, SummarySignatureReportConfig config = null)
		{
			Original = original;
			Config = config ?? SummarySignatureReportConfig.Default;
		}

		/// <summary>
		/// 元となる署名検証結果レポート（JSON）を取得します。
		/// </summary>
		protected string Original { get; }

		/// <summary>
		/// 署名検証結果サマリレポート設定を取得します。
		/// </summary>
		protected SummarySignatureReportConfig Config { get; }

		/// <summary>
		/// 署名検証結果サマリレポートをインデントでフォーマットするかどうか示す値を取得します。
		/// </summary>
		protected bool IsFormatting { get; private set; }

		/// <summary>
		/// 署名検証結果サマリレポート（JSON）を生成します。
		/// </summary>
		/// <returns>JSON <see cref="string"/> を返却します。</returns>
		public abstract string Build();

		/// <summary>
		/// 署名検証結果サマリレポートをインデントでフォーマットするかどうか示す値を設定します。
		/// </summary>
		/// <param name="enabled">フォーマットする場合に <c>true</c> を指定します。</param>
		/// <returns><see cref="SummarySignatureReportBuilder" /> クラスのインスタンス。デイジーチェーンで使用します。</returns>
		public SummarySignatureReportBuilder Format(bool enabled)
		{
			IsFormatting = enabled;

			return this;
		}

		/// <summary>
		/// <see cref="SummarySignatureReportBuilder"/> クラスの新しいインスタンスを作成します。
		/// </summary>
		/// <param name="original">検証結果レポート（JSON）を指定します。</param>
		/// <param name="config">サマリレポート作成で使用するオプションを指定します。</param>
		/// <returns><see cref="SummarySignatureReportBuilder"/> クラスの新しいインスタンス。</returns>
		public static SummarySignatureReportBuilder Create(string original, SummarySignatureReportConfig config = null)
		{
			return new MicrosoftSummarySignatureReportBuilder(original, config);
		}

	}
}
