using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace SignatureVerifier.TestUtilities
{
	internal static class AsymmetricCipherKeyPairGeneratorExtensions
	{

		public static AsymmetricCipherKeyPair GenerateECDSAKeyPair(this IAsymmetricCipherKeyPairGenerator generator,
			X9ECParameters curve = null
			)
		{
			curve = curve ?? NistNamedCurves.GetByName("P-521");

			var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
			generator.Init(new ECKeyGenerationParameters(domain, new SecureRandom()));

			return generator.GenerateKeyPair();
		}
	}
}
