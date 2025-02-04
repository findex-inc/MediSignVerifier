using System;
using System.Linq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.TestUtilities;


namespace SignatureVerifier.Verifiers
{
	partial class SignatureTimeStampVerifierTests
	{
		internal class Fixtures
		{
			// private is desirable.

			public (CertificateData, CertificatePathValidationData, CertificateData, byte[]) GetTestTSAData(DateTime utcDateTime, byte[] targetValue, bool isNormalData)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = isNormalData ? utcDateTime.AddSeconds(50) : utcDateTime.AddSeconds(-1);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test TSA Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test TSA Intermediate CA Certificate"), rootCert, 1L, before, after, interPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test TSA Certificate"), interCert, 1L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
							gen.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(KeyPurposeID.IdKPTimeStamping));
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

				var algorithm = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha512);
				var digest = DigestUtilities.CalculateDigest(algorithm.Algorithm, targetValue);
				var message = new MessageImprint(algorithm, digest);

				TimeStampToken tsToken = CreateTimeStampToken(
					message, serialNumber: 101L, utcDateTime, endPair, endCert,
					digestAlgorithmIdentifier: NistObjectIdentifiers.IdSha256);

				return (
					new CertificateData("#tsa", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("#trust", rootCert.GetEncoded()),
					tsToken.GetEncoded());
			}

			public static TimeStampToken CreateTimeStampToken(
				MessageImprint message,
				long serialNumber,
				DateTime genTime,
				AsymmetricCipherKeyPair tsaPair,
				X509Certificate tsaCert,
				DerObjectIdentifier digestAlgorithmIdentifier,
				X509Certificate[] certs = null,
				X509Crl[] crls = null)
			{
				var token = new TimeStampTokenGenerator(tsaPair.Private, tsaCert,
						digestOID: digestAlgorithmIdentifier.Id,
						tsaPolicyOID: TimeStampTokenGeneratorExtensions.TsaPolicyPUF5YearsExpiration.Id)
					.SetTsaCertificate(tsaCert)
					.Configure(
						gen =>
						{
							if (certs?.Any() ?? false) {

								IX509Store x509CertStore = X509StoreFactory.Create(
									"Certificate/Collection",
									new X509CollectionStoreParameters(certs));

								gen.SetCertificates(x509CertStore);
							}

							if (crls?.Any() ?? false) {

								IX509Store x509Crl = X509StoreFactory.Create(
									"Crl/Collection",
									new X509CollectionStoreParameters(crls));

								gen.SetCrls(x509Crl);
							}
						})
					.Generate(digestAlgorithmOid: message.HashAlgorithm.Algorithm.Id, message.GetHashedMessage(), nonce: null
						, BigInteger.ValueOf(serialNumber), genTime);

				TestContext.WriteLine(token.DumpAsString());

				return token;
			}

		}
	}
}
