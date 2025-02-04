using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.TestUtilities
{
	internal static class BasicOcspRespGeneratorExtensions
	{

		public static OcspResp GenerateOcspResp(this BasicOcspRespGenerator generator,
			int status, string signingAlgorithm, AsymmetricKeyParameter privateKey, X509Certificate cert, DateTime thisUpdate)
		{
			var basicResp = generator.Generate(signingAlgorithm, privateKey, new[] { cert }, thisUpdate, null);

			var respGenerator = new OCSPRespGenerator();
			var ocspResp = respGenerator.Generate(status, basicResp);

			return ocspResp;
		}

		public static BasicOcspRespGenerator Configure(this BasicOcspRespGenerator generator, Action<BasicOcspRespGenerator> action)
		{
			action(generator);

			return generator;
		}

	}
}
