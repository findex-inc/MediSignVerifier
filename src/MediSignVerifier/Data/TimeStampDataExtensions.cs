using System;
using System.Linq;

namespace SignatureVerifier.Data
{
	internal static class TimeStampDataExtensions
	{
		public static bool IsEqualsTimeStampHash(this TimeStampData data)
		{
			return data.HashValue.SequenceEqual(data.CalculatedValue);
		}

		public static bool IsEqualsMessageDigest(this TimeStampData data)
		{
			return data.MessageDigestValue.SequenceEqual(data.MessageCalculatedValue);
		}

		public static bool IsEqualsCertificateHash(this TimeStampData data)
		{
			return data.CertificateHashValue.SequenceEqual(data.CertificateCalculatedValue);
		}
	}
}
