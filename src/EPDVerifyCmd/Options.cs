using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace EPDVerifyCmd
{
	internal class Options
	{
		[Option('i', "in", Required = true, HelpText = "検証対象のファイルパス")]
		public string TargetPath { get; set; }

		[Option('o', "out", HelpText = "検証結果ファイル(JSON)のファイルパス")]
		public string OutputPath { get; set; }

		[Option('t', "verify-time", HelpText = "検証時刻")]
		public DateTime? VerificationTime { get; set; }

		[Option('q', "quiet", HelpText = "標準出力を抑制します。")]
		public bool Quiet { get; set; }

		[Option('f', "format", HelpText = "出力をフォーマット（インデント）して出力します。")]
		public bool FormatRequired { get; set; }

		[Option('r', "report", Separator = ',', HelpText = "[detail|summary]. 出力レポートの種類を指定します。','カンマ区切りで複数指定できます。")]
		public IEnumerable<ReportType> ReportTypes { get; set; }

		public enum ReportType
		{
			Detail = 0,
			Summary
		}

		[Option('c', "console", Default = ReportType.Summary, HelpText = "[detail|summary]. 標準出力の種類を指定します。")]
		public ReportType ConsoleType { get; set; }

		[Option("hpki", Default = true, HelpText = "HPKI固有の検証を有効にします。")]
		public bool? HPKIValidationEnabled { get; set; }


		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("Options: {");

			builder.Append($"{nameof(TargetPath)} = \"{TargetPath}\"");
			builder.Append($", {nameof(OutputPath)} = \"{OutputPath}\"");
			builder.Append($", {nameof(VerificationTime)} = \"{VerificationTime}\"");
			builder.Append($", {nameof(Quiet)} = {Quiet}");
			builder.Append($", {nameof(FormatRequired)} = {FormatRequired}");
			builder.Append($", {nameof(ReportTypes)} = \"{string.Join(",", ReportTypes)}\"");
			builder.Append($", {nameof(ConsoleType)} = \"{ConsoleType}\"");
			builder.Append($", {nameof(HPKIValidationEnabled)} = \"{HPKIValidationEnabled}\"");
			builder.Append('}');

			return builder.ToString();
		}
	}
}
