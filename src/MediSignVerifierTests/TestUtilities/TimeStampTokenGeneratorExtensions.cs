using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace SignatureVerifier.TestUtilities
{
	internal static class TimeStampTokenGeneratorExtensions
	{
		public static readonly DerObjectIdentifier TsaPolicyPUF5YearsExpiration = new DerObjectIdentifier("0.2.440.200185.1.1.1.1");

		public static TimeStampToken Generate(this TimeStampTokenGenerator generator,
			string digestAlgorithmOid, byte[] digest, BigInteger nonce,
			BigInteger serialNumber, DateTime genTime
			)
		{
			var requestGenerator = new TimeStampRequestGenerator();
			requestGenerator.SetCertReq(true);
			var request = requestGenerator.Generate(digestAlgorithmOid, digest, nonce);

			// client --- send: request  --> TSA server.

			var responseGenerator = new TimeStampResponseGenerator(generator, TspAlgorithms.Allowed);
			var response = responseGenerator.Generate(request, serialNumber, genTime);

			// client <-- recv: response --- TSA server.

			return response.TimeStampToken;
		}


		public static TimeStampTokenGenerator SetTsaCertificate(this TimeStampTokenGenerator generator, X509Certificate tsaCert)
		{
			IX509Store x509CertStore = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(new[] { tsaCert }));

			generator.SetCertificates(x509CertStore);

			return generator;
		}


		public static TimeStampTokenGenerator Configure(this TimeStampTokenGenerator generator, Action<TimeStampTokenGenerator> action)
		{
			action.Invoke(generator);

			return generator;
		}

	}
}
