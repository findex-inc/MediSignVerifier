using System;
using System.Collections.Generic;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.Reports.BouncyCastle
{
	internal static class X509CertificateExtensions
	{
		public static ICertificateInfo ToCertificateInfo(this X509Certificate cert, string source, DateTime validDate, IEnumerable<ICertificateRevocationInfo> revocations = null)
		{
			return new X509CertificateInfo(source, cert, validDate, revocations);
		}
	}
}
