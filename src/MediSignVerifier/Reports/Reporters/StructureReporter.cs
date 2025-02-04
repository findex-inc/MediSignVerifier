namespace SignatureVerifier.Reports.Reporters
{
	internal class StructureReporter
	{
#pragma warning disable IDE0052
		private readonly ReportConfig _config;
#pragma warning restore IDE0052

		public StructureReporter(ReportConfig config)
		{
			_config = config;
		}

		public StructureReport Generate(SignatureSourceType sourceType, VerificationResult result)
		{
			//resultがnullということは、Signature要素が一つもなかった＝INVALID
			return new StructureReport(result?.GetStatus(sourceType) ?? VerificationStatus.INVALID, result?.ToReportItems(sourceType));
		}
	}
}
