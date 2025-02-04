using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.TestUtilities;


namespace SignatureVerifier.Verifiers
{
	partial class SigningCertificateVerifierTests
	{

		private class Fixtures
		{

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestCertificatePathValidationData(DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 1L, before, after, interPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test Certificate"), interCert, 123L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.NonRepudiation));
						})
					.GenerateCertificate(interPair.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				var interCRL = new X509V2CrlGenerator()
					.Configure(interCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair.Private, "SHA256withECDSA");

				var end = endCert.GetEncoded();
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded(), endCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };

				return (
					new CertificateData("#signning", "#signning", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded())
					);

			}

		}

	}
}
