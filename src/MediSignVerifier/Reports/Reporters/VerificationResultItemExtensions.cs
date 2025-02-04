namespace SignatureVerifier.Reports.Reporters
{
	internal static class VerificationResultItemExtensions
	{
		public static VerificationResultReportItem ToReport(this VerificationResultItem item)
		{
			return new VerificationResultReportItem(item.ItemName, item.Status, item.Message);
		}
	}
}
