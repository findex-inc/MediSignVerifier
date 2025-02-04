using System;
using System.Diagnostics;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace EPDVerifyCmd
{
	internal class Program
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			try {
				_logger.Info("Start.");
				var stopwatch = Stopwatch.StartNew();

				var options = AnalyzeArgs(args);
				if (options == null) {

					Environment.ExitCode = -1;
					return; //引数変換に失敗したため終了
				}

				IServiceCollection serviceCollection = new ServiceCollection();
				ConfigureServices(serviceCollection);

				using var servicesProvider = serviceCollection.BuildServiceProvider();

				servicesProvider.GetRequiredService<Runner>().DoAction(options);

				stopwatch.Stop();
				_logger.Info($"Exit({Environment.ExitCode}), Elapsed: {stopwatch.Elapsed}");
			}
			catch (Exception ex) {

				Console.Error.WriteLine(ex.ToString());
				_logger.Error(ex, "エラーが発生しました。");
				Environment.ExitCode = -1;
			}
			finally {

				LogManager.Shutdown();
			}
		}

		static Options AnalyzeArgs(string[] args)
		{
			Options options = null;
			var parser = new Parser(with =>
			{
				with.CaseInsensitiveEnumValues = true;
				with.HelpWriter = Console.Error;
			});
			var parseResult = parser.ParseArguments<Options>(args);
			parseResult.WithParsed(opt =>
			{
				options = opt;
				options.VerificationTime ??= DateTime.Now;

				_logger.Debug($"Options：{options}");
			});

			parseResult.WithNotParsed(errs =>
			{
				// As a reference.
				//https://github.com/commandlineparser/commandline/blob/d443a51aeb3a418425e970542b3b96e9da5f62e2/demo/ReadText.LocalizedDemo/LocalizableSentenceBuilder.cs

				var builder = SentenceBuilder.Factory();
				var messages = errs
					.Where(x => x is not HelpRequestedError)
					.Select(x => builder.FormatError(x));

				foreach (var message in messages) {

					_logger.Error(message);
				}

			});

			return options;
		}

		static void ConfigureServices(IServiceCollection services)
		{
			services.AddTransient<Runner>();
		}
	}
}
