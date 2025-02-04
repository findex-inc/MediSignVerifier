using System;
using System.Collections.Generic;
using System.Linq;
using SignatureVerifier.Data;

namespace SignatureVerifier.TestUtilities
{
	internal static class CertificateResource
	{
		public static IEnumerable<CertificateData> TrustAnchors() => _trustAnchers.Value;

		private static readonly Lazy<IEnumerable<CertificateData>> _trustAnchers
			= new Lazy<IEnumerable<CertificateData>>(() =>
			{
				var certBytes = new[]
				{
					global::MediSignVerifier.Tests.Properties.Resources.TestCA1.RemoveUTF8BOM(),
					global::MediSignVerifier.Tests.Properties.Resources.TestTSACA1.RemoveUTF8BOM(),
				};

				var parser = new CertificateDataParser("<Trusted test resource>");

				return certBytes.Select(x => parser.ReadCertificateData(x)).ToArray();
			});

	}
}
