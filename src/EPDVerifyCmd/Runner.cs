using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using SignatureVerifier;

#pragma warning disable CA1822

namespace EPDVerifyCmd
{

	internal class Runner
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public Runner()
		{
		}

		public void DoAction(Options options)
		{
			_logger.Trace("Start.");

			ValidateOptions(options);

			var doc = SignedDocumentXml.Load(options.TargetPath);

			var vconfig = new VerificationConfig()
			{
				HPKIValidationEnabled = options.HPKIValidationEnabled!.Value,
			};
			var result = VerifyDocument(options, vconfig, doc);

			var rconfig = new ReportConfig();
			var report = GenerateReport(options, rconfig, doc, result);

			WriteReport(options, report);

			var exitcode = (int)result.Status;
			Environment.ExitCode = exitcode;
			_logger.Info($"検証結果: {result.Status}");

			_logger.Trace($"End");
			return;
		}


		private static SignatureVerificationResult VerifyDocument(Options options, VerificationConfig config, SignedDocumentXml doc)
		{
			var verifier = new SignatureVerifier.SignatureVerifier(config);
			verifier.VerifiedEvent += OnVerifiedEvent;

			var result = verifier.Verify(doc, options.VerificationTime.Value);

			return result;

			static void OnVerifiedEvent(object sender, VerifiedEventArgs e)
			{
				_logger.Warn($"Accept event [{e.Status}]: -> {e.Message}");
			}

		}


		private static SignatureVerificationReport GenerateReport(Options options, ReportConfig config, SignedDocumentXml doc, SignatureVerificationResult result)
		{
			var reporter = new SignatureReporter(config);

			var report = reporter.Generate(doc, options.VerificationTime.Value, result);

			return report;
		}

		private static void WriteReport(Options options, SignatureVerificationReport report)
		{
			var detailJson = report
				.ConfigureJsonSettings(settings =>
				{
					settings.FormatRequired = options.FormatRequired;
					settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
				})
				.ToJson();

			string summaryJson = null;

			if (options.OutputPath != null) {

				var dir = Path.GetDirectoryName(options.OutputPath);
				if ((!string.IsNullOrEmpty(dir)) && (!Directory.Exists(dir))) {

					Directory.CreateDirectory(dir);
					_logger.Trace($"created \"{dir}\"");
				}

				if (options.ReportTypes.Any()) {

					if (options.ReportTypes.Contains(Options.ReportType.Summary)) {

						summaryJson ??= SummarySignatureReportBuilder.Create(detailJson)
							.Format(options.FormatRequired)
							.Build();

						var path = RenameReportFilePath(options.OutputPath, nameof(Options.ReportType.Summary));
						File.WriteAllText(path, summaryJson, Encoding.UTF8);
						_logger.Info($"File written. \"{path}\"");
					}

					if (options.ReportTypes.Contains(Options.ReportType.Detail)) {

						var path = RenameReportFilePath(options.OutputPath, nameof(Options.ReportType.Detail));
						File.WriteAllText(path, detailJson, Encoding.UTF8);
						_logger.Info($"File written. \"{path}\"");
					}
				}
				else {

					var path = options.OutputPath;
					File.WriteAllText(path, detailJson, Encoding.UTF8);
					_logger.Info($"File written. \"{path}\"");
				}
			}

			if (!options.Quiet) {

				if (options.ConsoleType == Options.ReportType.Summary) {

					summaryJson ??= SummarySignatureReportBuilder.Create(detailJson)
						.Format(options.FormatRequired)
						.Build();

					Console.WriteLine(summaryJson);
				}
				else {

					Console.WriteLine(detailJson);
				}
			}

			return;
		}


		private static void ValidateOptions(Options options)
		{
			if (!File.Exists(options.TargetPath)) {

				throw new FileNotFoundException("検証対象のファイルが見つかりません", options.TargetPath);
			}

			//if (options.OutputPath != null) {

			//	if (File.Exists(options.OutputPath)) {

			//		throw new IOException($"同名のファイルが存在しています。\"{options.TargetPath}\"");
			//	}
			//}

			return; // success
		}


		private static string RenameReportFilePath(string basepath, string type)
		{
			var dir = Path.GetDirectoryName(basepath);
			var basename = Path.GetFileNameWithoutExtension(basepath);
			var extension = Path.GetExtension(basepath);

			var newfilename = $"{basename}_{type.ToLower()}{extension}";
			var newpath = Path.Combine(dir, newfilename);

			return newpath;
		}

	}
}
