using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using SignatureVerifier.Reports.Serialization;

namespace SignatureVerifier
{
	/// <summary>
	/// <see cref="SignatureVerificationReport"/> のための拡張メソッドです。
	/// </summary>
	public static class SignatureVerificationReportExtensions
	{
		/// <summary>
		/// メソッド <see cref="SignatureVerificationReportExtensions.ToJson(SignatureVerificationReport, SignatureVerificationReportSettings)"/>
		/// を使用するための設定を行います。
		/// </summary>
		/// <param name="report">署名検証結果レポート</param>
		/// <param name="action"><see cref="SignatureVerificationReportSettings" />を構成するためのデリゲートを指定します。</param>
		/// <returns>署名検証結果レポートとオプションのタプルを返却します。
		/// この返却値は<see cref="SignatureVerificationReportExtensions.ToJson(SignatureVerificationReport, SignatureVerificationReportSettings)"/>
		/// を使用するためのものです。</returns>
		public static (SignatureVerificationReport, SignatureVerificationReportSettings) ConfigureJsonSettings(
			this SignatureVerificationReport report,
			Action<SignatureVerificationReportSettings> action = default)
		{
			var settings = new SignatureVerificationReportSettings();
			action?.Invoke(settings);

			return (report, settings);
		}

		/// <summary>
		/// 指定された値をJSON <see cref="string"/> に変換します。
		/// </summary>
		/// <param name="builder"><see cref="SignatureVerificationReport" /> と <see cref="SignatureVerificationReportSettings"/> のタプル</param>
		/// <returns>JSON <see cref="string"/> を返却します。</returns>
		public static string ToJson(this (SignatureVerificationReport Report, SignatureVerificationReportSettings Settings) builder)
			=> builder.Report.ToJson(builder.Settings);

		/// <summary>
		/// 指定された値をJSON <see cref="string"/> に変換します。
		/// </summary>
		/// <param name="report"><see cref="SignatureVerificationReport" />	クラスのインスタンス</param>
		/// <param name="settings">レポートのオプション</param>
		/// <returns>JSON <see cref="string"/> を返却します。</returns>
		public static string ToJson(this SignatureVerificationReport report, SignatureVerificationReportSettings settings = default)
			=> JsonSerializer.Serialize(report, options: settings?.ToJsonSerializerOptions());

		/// <summary>
		/// 指定された <see cref="SignatureVerificationReportSettings"/> を <see cref="JsonSerializerOptions"/> に変換します。
		/// </summary>
		/// <param name="settings">レポートのオプション</param>
		/// <returns>設定済みの<see cref="JsonSerializerOptions"/>インスタンス。</returns>
		public static JsonSerializerOptions ToJsonSerializerOptions(this SignatureVerificationReportSettings settings)
		{
			var options = CreateOptions();
			options.WriteIndented = settings.FormatRequired;
			options.Converters.Add(new ZoneHandlingDateTimeConvertor(settings.DateTimeZoneHandling));

			return options;

			JsonSerializerOptions CreateOptions()
			{
				var opt = new JsonSerializerOptions
				{
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				};
				opt.Converters.Add(new JsonStringEnumConverter());

				return opt;
			}
		}

		/// <summary>
		/// メソッド <see cref="SignatureVerificationReportExtensions.ToJson(SignatureVerificationReport, SignatureVerificationReportSettings)"/>
		/// で使用するオプションを提供します。
		/// </summary>
		public class SignatureVerificationReportSettings
		{
			/// <summary>
			/// JSONをきれいに出力するかどうかを定義します。
			/// </summary>
			public bool FormatRequired { get; set; }

			/// <summary>
			/// シリアル化および逆シリアル化中に DateTime タイム ゾーンがどのように処理されるかを取得または設定します。
			/// デフォルト値は <see cref="DateTimeZoneHandling.RoundtripKind" /> です。
			/// </summary>
			public DateTimeZoneHandling DateTimeZoneHandling { get; set; } = DateTimeZoneHandling.RoundtripKind;
		}

	}
}
