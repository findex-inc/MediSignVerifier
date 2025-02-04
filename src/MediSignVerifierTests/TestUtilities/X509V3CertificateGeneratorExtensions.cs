using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace SignatureVerifier.TestUtilities
{
	internal static class X509V3CertificateGeneratorExtensions
	{

		public static X509Certificate GenerateCertificate(this X509V3CertificateGenerator generator,
			AsymmetricKeyParameter caKey,
			string algorithm
			)
		{
			ISignatureFactory signatureFactory =
				new Asn1SignatureFactory(algorithm, caKey, new SecureRandom());

			return generator.Generate(signatureFactory);
		}


		public static X509V3CertificateGenerator Configure(this X509V3CertificateGenerator generator, Action<X509V3CertificateGenerator> action)
		{
			action.Invoke(generator);
			return generator;
		}

		public static X509V3CertificateGenerator ConfigureTestRootCA(this X509V3CertificateGenerator generator,
			X509Name rootName,
			ulong serial,
			DateTime before,
			DateTime after,
			AsymmetricKeyParameter publicKey,
			Action<X509V3CertificateGenerator> action = null)
		{
			generator.Configure(gen =>
			{
				gen.SetIssuerDN(rootName);
				gen.SetSerialNumber(new BigInteger(serial.ToString(), 10));
				gen.SetNotBefore(before);
				gen.SetNotAfter(after);
				gen.SetSubjectDN(rootName);
				gen.SetPublicKey(publicKey);

				gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(publicKey));
				gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(cA: true));
			});

			action?.Invoke(generator);

			return generator;
		}

		public static X509V3CertificateGenerator ConfigureTestCA(this X509V3CertificateGenerator generator,
			X509Name subject,
			X509Certificate issuer,
			ulong serial,
			DateTime before,
			DateTime after,
			AsymmetricKeyParameter publicKey,
			int pathLenConstraint = 0,
			Action<X509V3CertificateGenerator> action = null)
		{
			generator.Configure(gen =>
			{
				gen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(issuer));
				gen.SetSerialNumber(new BigInteger(serial.ToString(), 10));
				gen.SetNotBefore(before);
				gen.SetNotAfter(after);
				gen.SetSubjectDN(subject);
				gen.SetPublicKey(publicKey);

				gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(publicKey));
				gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(issuer));
				gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(pathLenConstraint));
				gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));
			});

			action?.Invoke(generator);

			return generator;
		}

		public static X509V3CertificateGenerator ConfigureEndEntity(this X509V3CertificateGenerator generator,
			X509Name subject,
			X509Certificate issuer,
			ulong serial,
			DateTime before,
			DateTime after,
			AsymmetricKeyParameter publicKey,
			Action<X509V3CertificateGenerator> action = null)
		{
			generator.Configure(gen =>
			{
				gen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(issuer));
				gen.SetSerialNumber(new BigInteger(serial.ToString(), 10));
				gen.SetNotBefore(before);
				gen.SetNotAfter(after);
				gen.SetSubjectDN(subject);
				gen.SetPublicKey(publicKey);

				gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(publicKey));
				gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(issuer));
				gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(cA: false));
			});

			action?.Invoke(generator);

			return generator;
		}
	}
}
