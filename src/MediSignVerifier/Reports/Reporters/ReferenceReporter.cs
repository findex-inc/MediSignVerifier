using System.Collections.Generic;
using System.Linq;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Reports.Reporters
{
	internal class ReferenceReporter
	{
#pragma warning disable IDE0052
		private readonly ReportConfig _config;
#pragma warning restore IDE0052

		public ReferenceReporter(ReportConfig config)
		{
			_config = config;
		}

		public ReferenceReportList Generate(ISignature signature, VerificationResult result)
		{
			//Index == -1 のものは、検証不能なエラーのはずで複数は出ない想定
			var commonError = result.ToReportItems(signature.SourceType, -1).FirstOrDefault();
			var reports = new List<ReferenceReport>();

			if (signature.ReferenceValidationData != null) {
				foreach (var reference in signature.ReferenceValidationData) {
					var resultItems = result.ToReportItems(signature.SourceType, reference.DispIndex);
					var status = result.GetStatus(signature.SourceType, reference.DispIndex);

					var report = ReferenceReport.Create(
						reference.DispIndex,
						status,
						reference.Id,
						reference.Uri,
						reference.Transform?.C14nAlgorithmName() ?? "default(C14N)",
						reference.DigestMethod?.ToDerObjectIdentifier()?.DigestAlgorithmName(),
						reference.DigestValue?.ToBase64String(),
						reference.CalculatedValue?.ToBase64String(),
						resultItems
						);

					reports.Add(report);
				}
			}

			return ReferenceReportList.Create(result.GetStatus(signature.SourceType), commonError?.Message, reports);
		}
	}
}
