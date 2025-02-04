using System.Collections.Generic;
using System.Linq;

namespace SignatureVerifier.Verifiers
{
	internal static class VerificationStatusExtensions
	{
		public static VerificationStatus ToConclusion(this IEnumerable<VerificationStatus> status,
			VerificationStatus defaultStatus = VerificationStatus.VALID)
		{
			if (!status.Any()) {

				return defaultStatus;
			}

			if (status.Any(x => x == VerificationStatus.INVALID)) {

				return VerificationStatus.INVALID;
			}

			if (status.Any(x => x == VerificationStatus.INDETERMINATE)) {


				return VerificationStatus.INDETERMINATE;
			}

			return VerificationStatus.VALID;
		}
	}
}
