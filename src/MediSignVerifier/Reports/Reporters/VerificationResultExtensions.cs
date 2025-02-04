using System.Collections.Generic;
using System.Linq;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal static class VerificationResultExtensions
	{
		public static VerificationResultReportItem[] ToReportItems(this VerificationResult source, SignatureSourceType sourceType, int index = -1)
		{
			//TODO エラーのみ出すというフィルタオプション必要？（VALIDは除外）

			var list = new List<VerificationResultReportItem>();

			var query = source.FilterItems(sourceType, index)
				.GroupBy(m => new { m.ItemName, m.Status });
			foreach (var group in query) {
				foreach (var item in group) {
					list.Add(item.ToReport());
				}
			}

			return list.ToArray();
		}

		public static VerificationStatus GetStatus(this VerificationResult source, SignatureSourceType sourceType)
		{
			return source.Items.Where(m => m.Type == sourceType).Select(m => m.Status).ToConclusion();
		}

		public static VerificationStatus GetStatus(this VerificationResult source, SignatureSourceType sourceType, int index)
		{
			return source.FilterItems(sourceType, index).Select(m => m.Status).ToConclusion();
		}

		private static IEnumerable<VerificationResultItem> FilterItems(this VerificationResult source, SignatureSourceType sourceType, int index)
		{
			return source.Items.Where(m => m.Type == sourceType && m.Index == index);
		}
	}
}
