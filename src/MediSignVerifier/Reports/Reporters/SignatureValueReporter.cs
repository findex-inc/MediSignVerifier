using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SignatureValueReporter
	{
#pragma warning disable IDE0052
		private readonly ReportConfig _config;
#pragma warning restore IDE0052

		public SignatureValueReporter(ReportConfig config)
		{
			_config = config;
		}

		public SignatureValueReport Generate(ISignature signature, VerificationResult result)
		{
			var target = signature.SignatureValueValidationData;
			var resultItems = result.ToReportItems(signature.SourceType);
			var status = result.GetStatus(signature.SourceType);

			return SignatureValueReport.Create(
				status,
				target?.Id,
				target?.CanonicalizationMethod?.C14nAlgorithmName(),
				target?.SignatureMethod?.ToBCMechanism()?.SignatureAlgorithmName() ?? target?.SignatureMethod,
				resultItems
				);
		}
	}
}
