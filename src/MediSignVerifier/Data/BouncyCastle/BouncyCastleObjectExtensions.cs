using System;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class BouncyCastleObjectExtensions
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public static X509Certificate ToX509Certificate(this byte[] source)
		{
			return new X509Certificate(source);
		}

		public static X509Certificate ToX509CertificateOrDefault(this byte[] source)
		{
			try {

				return source.ToX509Certificate();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is CertificateParsingException) {

				_logger.Debug(ex, "Ignore exceptions, return null.");

				return null;
			}
		}

		public static X509Crl ToX509Crl(this byte[] source)
		{
			return new X509Crl(source);
		}

		public static X509Crl ToX509CrlOrDefault(this byte[] source)
		{
			try {

				return source.ToX509Crl();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is CrlException) {

				_logger.Debug(ex, "Ignore exceptions, return null.");

				return null;
			}
		}

		public static OcspResp ToOcspResp(this byte[] source)
		{
			return new OcspResp(OcspResponse.GetInstance(Asn1Sequence.FromByteArray(source)));
		}

		public static OcspResp ToOcspRespOrDefault(this byte[] source)
		{
			try {

				return source.ToOcspResp();
			}
			catch (Exception ex) when (ex is ArgumentException) {

				_logger.Debug(ex, "Ignore exceptions, return null.");

				return null;
			}
		}


		public static CmsSignedData ToCmsSignedData(this byte[] source)
		{
			return new CmsSignedData(source);
		}

		public static TimeStampToken ToTimeStampToken(this byte[] source)
		{
			return new TimeStampToken(new CmsSignedData(source));
		}

		public static byte[] CalculateDigest(this byte[] source, DerObjectIdentifier algorithm)
		{
			return DigestUtilities.CalculateDigest(algorithm, source);
		}

		public static X509Name ToX509Name(this string source)
		{
			return new X509Name(source);
		}

		public static IssuerSerial ToX509IssuerSerial(this byte[] source)
		{
			return IssuerSerial.GetInstance(Asn1Sequence.FromByteArray(source));
		}

		public static BigInteger ToBigInteger(this string source, int radix = 16)
		{
			return new BigInteger(source, radix);
		}

	}
}
