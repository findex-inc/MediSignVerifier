using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace SignatureVerifier.TestUtilities
{
	internal static class X509V2CrlGeneratorExtensions
	{
		public static X509Crl GenerateCrl(this X509V2CrlGenerator generator,
			AsymmetricKeyParameter caKey,
			string algorithm
			)
		{
			ISignatureFactory signatureFactory =
				new Asn1SignatureFactory(algorithm, caKey, new SecureRandom());

			return generator.Generate(signatureFactory);
		}

		public static X509V2CrlGenerator Configure(this X509V2CrlGenerator generator, Action<X509V2CrlGenerator> action)
		{
			action.Invoke(generator);
			return generator;
		}

		public static X509V2CrlGenerator Configure(this X509V2CrlGenerator generator,
			X509Certificate issuer,
			DateTime updatedAt,
			DateTime nextUpdatedAt,
			Action<X509V2CrlGenerator> action = null)
		{
			generator.Configure(gen =>
			{
				gen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(issuer));
				gen.SetThisUpdate(updatedAt);
				gen.SetNextUpdate(nextUpdatedAt);

				gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(issuer));
			});

			action?.Invoke(generator);

			return generator;
		}
	}
}
