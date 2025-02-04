using Org.BouncyCastle.Asn1.X509;

namespace SignatureVerifier.TestUtilities
{
	internal static class GeneralNameExtensions
	{
		public static DistributionPoint ToDistributionPointWithDistributionPointName(this GeneralName source)
		{
			return new DistributionPoint(
				new DistributionPointName(new GeneralNames(source)),
				null,
				null);
		}

		public static DistributionPoint ToDistributionPointWithCrlIssuer(this GeneralName source)
		{
			return new DistributionPoint(
				null,
				null,
				new GeneralNames(source));
		}

	}
}
