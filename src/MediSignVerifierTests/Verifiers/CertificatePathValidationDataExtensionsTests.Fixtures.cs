using System;
using System.Collections.Generic;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.TestUtilities;


namespace SignatureVerifier.Verifiers
{
	partial class CertificatePathValidationDataTests
	{
		internal class Fixtures
		{
			// private is desirable.

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestPathValidationData(DateTime utcDateTime)
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
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 1L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
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
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}


			public enum InvalidStructureType
			{
				対象証明書不正データ,
				証明書リストnullデータ,
				証明書リスト不正データ,
				失効情報リストnullデータ,
				失効情報リスト不正データ,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForInvalidStructure(InvalidStructureType type, DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), rootCert, 1L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				(CertificateData, CertificatePathValidationData, CertificateData) pathData;
				switch (type) {
					case InvalidStructureType.対象証明書不正データ:
						pathData = (
							new CertificateData("unknown", new byte[] { 0x00, 0x00, 0x00, 0x00 }),
							new CertificatePathValidationData(
								"unknown",
								new[] { rootCert.GetEncoded() },
								new[] { rootCRL.GetEncoded() }),
							new CertificateData("trust", rootCert.GetEncoded()));
						break;

					case InvalidStructureType.証明書リストnullデータ:
						pathData = (
							new CertificateData("unknown", endCert.GetEncoded()),
							new CertificatePathValidationData(
								"unknown",
								new byte[][] { null },
								new[] { rootCRL.GetEncoded() }),
							new CertificateData("trust", rootCert.GetEncoded()));
						break;

					case InvalidStructureType.証明書リスト不正データ:
						pathData = (
							new CertificateData("unknown", endCert.GetEncoded()),
							new CertificatePathValidationData(
								"unknown",
								new[] { new byte[] { 0x00, 0x00, 0x00, 0x00 } },
								new[] { rootCRL.GetEncoded() }),
							new CertificateData("trust", rootCert.GetEncoded()));
						break;

					case InvalidStructureType.失効情報リストnullデータ:
						pathData = (
							new CertificateData("unknown", endCert.GetEncoded()),
							new CertificatePathValidationData(
								"unknown",
								new[] { rootCert.GetEncoded() },
								new byte[][] { null }),
							new CertificateData("trust", rootCert.GetEncoded()));
						break;

					case InvalidStructureType.失効情報リスト不正データ:
						pathData = (
							new CertificateData("unknown", endCert.GetEncoded()),
							new CertificatePathValidationData(
								"unknown",
								new[] { rootCert.GetEncoded() },
								new[] { new byte[] { 0x00, 0x00, 0x00, 0x00 } }),
							new CertificateData("trust", rootCert.GetEncoded()));
						break;

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};

				return pathData;
			}

			public CertificatePathValidationException GetExpectedExceptionForInvalidStructure(InvalidStructureType type)
			{
				switch (type) {
					case InvalidStructureType.対象証明書不正データ:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の構造が正しくありません。",
								new ArgumentException("failed to construct sequence from byte[]: unexpected end-of-contents marker"));

					case InvalidStructureType.証明書リストnullデータ:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の構造が正しくありません。",
								new CertificateParsingException("Certificate contents invalid: System.NullReferenceException: Object reference not set to an instance of an object.\r\n   at Org.BouncyCastle.X509.X509Certificate..ctor(X509CertificateStructure c)"));

					case InvalidStructureType.証明書リスト不正データ:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の構造が正しくありません。",
								new ArgumentException("failed to construct sequence from byte[]: unexpected end-of-contents marker"));

					case InvalidStructureType.失効情報リストnullデータ:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"失効情報(CRL)の構造が正しくありません。",
								new CrlException("CRL contents invalid: System.NullReferenceException: Object reference not set to an instance of an object.\r\n   at Org.BouncyCastle.X509.X509Crl..ctor(CertificateList c)"));

					case InvalidStructureType.失効情報リスト不正データ:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"失効情報(CRL)の構造が正しくありません。",
								new ArgumentException("failed to construct sequence from byte[]: unexpected end-of-contents marker"));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}

			// 6.1.4
			// If check (a), (k), (l), (n), or (o) fails, the procedure terminates,
			// returning a failure indication and an appropriate reason.

			public enum InvalidConstraintType
			{
				PolicyMappings1,
				PolicyMappings2,
				BasicConstraints_CaFlag1,
				BasicConstraints_CaFlag2,
				BasicConstraints_PathLength,
				KeyUsage,
				UnknownClitical,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForInvalidConstraint(InvalidConstraintType type, DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair1 = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert1 = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test 01 Intermediate CA Certificate"), rootCert, 1L, before, after, interPair1.Public,
						// (l) basicConstraints extension - pathLenConstraint
						pathLenConstraint: (type == InvalidConstraintType.BasicConstraints_PathLength) ? 0 : 1
						)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair2 = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert2 = new X509V3CertificateGenerator()
						.Configure(gen =>
						{
							gen.SetSerialNumber(BigInteger.One);
							gen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(interCert1));
							gen.SetNotBefore(before);
							gen.SetNotAfter(after);
							gen.SetSubjectDN(new X509Name("CN=Test 02 Intermediate CA Certificate"));
							gen.SetPublicKey(interPair2.Public);

							gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(interPair2.Public));
							gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(interCert1));

							// (k) basicConstraints extension - cA 
							if (type == InvalidConstraintType.BasicConstraints_CaFlag1) {
								gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(cA: false));
							}
							else if (type == InvalidConstraintType.BasicConstraints_CaFlag2) {
								//NOT X509Extensions.BasicConstraints
							}
							else {
								gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(pathLenConstraint: 0));
							}

							// (n) key usage extension
							if (type == InvalidConstraintType.KeyUsage) {
								gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.CrlSign));
							}
							else {
								gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));
							}

							// (a) policy mappings extension
							if (type == InvalidConstraintType.PolicyMappings1) {

								gen.AddExtension(X509Extensions.PolicyMappings, true,
									new PolicyMappings(new Dictionary<string, string>
									{
										[ /* ANY_POLICY */ "2.5.29.32.0"] = /* ANY_POLICY */ "2.5.29.32.0",
										[ /* id-TEST-certPolicyOne */ "1.3.6.1.5.5.7.13.1"] = /* id-TEST-certPolicyEight */ "1.3.6.1.5.5.7.13.8",
									}));
							}
							else if (type == InvalidConstraintType.PolicyMappings2) {

								gen.AddExtension(X509Extensions.PolicyMappings, true,
									new PolicyMappings(new Dictionary<string, string>
									{
										[ /* id-TEST-certPolicyOne */ "1.3.6.1.5.5.7.13.1"] = /* id-TEST-certPolicyOne */ "1.3.6.1.5.5.7.13.1",
										[ /* id-TEST-certPolicyEight */ "1.3.6.1.5.5.7.13.8"] = /* ANY_POLICY */ "2.5.29.32.0",
									}));
							}

						})
					.GenerateCertificate(interPair1.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert2, 1L, before, after, endPair.Public,
						gen =>
						{
							// Unknown Clitical
							if (type == InvalidConstraintType.UnknownClitical) {
								gen.AddExtension(new DerObjectIdentifier(/* Obsolete */"2.5.29.11"), true, new DerUtf8String("Unknown Clitical"));
							}
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(interPair2.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				var interCRL1 = new X509V2CrlGenerator()
					.Configure(interCert1, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair1.Private, "SHA256withECDSA");

				var interCRL2 = new X509V2CrlGenerator()
					.Configure(interCert2, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair2.Private, "SHA256withECDSA");

				var end = endCert.GetEncoded();
				var certs = new[] { rootCert.GetEncoded(), interCert1.GetEncoded(), interCert2.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL1.GetEncoded(), interCRL2.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}

			public CertificatePathValidationException GetExpectedExceptionForInvalidConstraint(InvalidConstraintType type)
			{
				switch (type) {
					case InvalidConstraintType.PolicyMappings1:
						// RFC 5280 6.1.4 (k)
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("IssuerDomainPolicy is anyPolicy")));

					case InvalidConstraintType.PolicyMappings2:
						// RFC 5280 6.1.4 (k)
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("SubjectDomainPolicy is anyPolicy,")));

					case InvalidConstraintType.BasicConstraints_CaFlag1:
						// RFC 5280 6.1.4 (k)
						// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs#L1823
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Not a CA certificate")));

					case InvalidConstraintType.BasicConstraints_CaFlag2:
						// RFC 5280 6.1.4 (k)
						// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs#L1827
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Intermediate certificate lacks BasicConstraints")));

					case InvalidConstraintType.BasicConstraints_PathLength:
						// RFC 5280 6.1.4 (l)
						// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs#L1846
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Max path length not greater than zero")));

					case InvalidConstraintType.KeyUsage:
						// RFC 5280 6.1.4 (n)
						// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs#L1910
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Issuer certificate keyusage extension is critical and does not permit key signing.")));

					case InvalidConstraintType.UnknownClitical:
						// RFC 5280 6.1.4 (o)
						// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs#L2109
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の制約を満足していません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Certificate has unsupported critical extension")));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}


			public enum InvalidPathBuildType
			{
				//署名者証明書があること（証明書を直接入れているのでありえない）
				上位証明書がない,
				//トラストアンカーに辿りつかない(上位証明書がないになる),
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForInvalidPathBuild(InvalidPathBuildType type, DateTime utcDateTime)
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
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 1L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(interPair.Private, "SHA256withECDSA");

				var otherRootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var otherRootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Other Root CA Certificate"), 1L, before, after, otherRootPair.Public)
					.GenerateCertificate(otherRootPair.Private, "SHA256withECDSA");

				var end = endCert.GetEncoded();
				byte[][] certs;
				switch (type) {
					case InvalidPathBuildType.上位証明書がない:
						certs = new[] { rootCert.GetEncoded() /*, interCert.GetEncoded() */ };

						return (
							new CertificateData("unknown", end),
							new CertificatePathValidationData("unknown", certs, null),
							new CertificateData("other", rootCert.GetEncoded()));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};


			}

			public CertificatePathValidationException GetExpectedExceptionForInvalidPathBuild(InvalidPathBuildType type)
			{
				switch (type) {
					case InvalidPathBuildType.上位証明書がない:
						return new CertificatePathValidationException(VerificationStatus.INDETERMINATE,
								"上位証明書が見つかりません。",
								new PkixCertPathBuilderException("No issuer certificate for certificate in certification path found."));

					//case InvalidPathBuildType.トラストアンカーに辿りつかない:
					//	return new CertificatePathValidationException(VerificationStatus.INDETERMINATE,
					//					"トラストアンカーにたどりつくことができません。",
					//			new PkixCertPathBuilderException("Unable to find certificate chain."));
					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}


			public enum InvalidSigunatureType
			{
				CA,
				EndEntity,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForInvalidSigunature(InvalidSigunatureType type, DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				// another pair sign.
				var anotherPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 1L, before, after, interPair.Public)
					.GenerateCertificate(
						type == InvalidSigunatureType.CA ? anotherPair.Private : rootPair.Private,
						"SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 1L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(
						type == InvalidSigunatureType.EndEntity ? anotherPair.Private : interPair.Private,
						"SHA256withECDSA");

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
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}

			public CertificatePathValidationException GetExpectedExceptionForInvalidSigunature(InvalidSigunatureType type)
			{
				switch (type) {
					case InvalidSigunatureType.CA:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の署名検証が失敗しました。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Could not validate certificate signature.")));

					case InvalidSigunatureType.EndEntity:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書の署名検証が失敗しました。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Could not validate certificate signature.")));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}


			public enum RevokedType
			{
				CAwithCRL,
				CAwithOCSP,
				EndEntitywithCRL,
				EndEntitywithOCSP,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForRevoked(RevokedType type, DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 100L, before, after, interPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 200L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(interPair.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							if (type == RevokedType.CAwithCRL) {
								gen.AddCrlEntry(BigInteger.ValueOf(100L), utcDateTime, CrlReason.AACompromise);
							}

							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				var interCRL = new X509V2CrlGenerator()
					.Configure(interCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);

							if (type == RevokedType.EndEntitywithCRL) {
								gen.AddCrlEntry(BigInteger.ValueOf(200L), utcDateTime, CrlReason.Superseded);
							}

							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair.Private, "SHA256withECDSA");

				var rootOCSP = new BasicOcspRespGenerator(interPair.Public)
					.Configure(
						gen =>
						{
							if (type == RevokedType.CAwithOCSP) {

								var id = new CertificateID(CertificateID.HashSha1, rootCert, interCert.SerialNumber);
								gen.AddResponse(id, new RevokedStatus(utcDateTime, CrlReason.AACompromise));
							}
							else {
								var id = new CertificateID(CertificateID.HashSha1, rootCert, BigInteger.One);
								gen.AddResponse(id, CertificateStatus.Good);
							}
						})
					.GenerateOcspResp(OCSPRespGenerator.Successful,
						"SHA256withECDSA", interPair.Private, interCert, utcDateTime);

				TestContext.WriteLine(rootOCSP.DumpAsString());

				var interOCSP = new BasicOcspRespGenerator(endPair.Public)
					.Configure(
						gen =>
						{
							if (type == RevokedType.EndEntitywithOCSP) {

								var id = new CertificateID(CertificateID.HashSha1, interCert, endCert.SerialNumber);
								gen.AddResponse(id, new RevokedStatus(utcDateTime, CrlReason.Superseded));
							}
							else {
								var id = new CertificateID(CertificateID.HashSha1, interCert, BigInteger.One);
								gen.AddResponse(id, CertificateStatus.Good);
							}
						})
					.GenerateOcspResp(OCSPRespGenerator.Successful,
						"SHA256withECDSA", endPair.Private, endCert, utcDateTime);

				TestContext.WriteLine(interOCSP.DumpAsString());

				var end = endCert.GetEncoded();
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };
				var ocsp = new[] { rootOCSP.GetEncoded(), interOCSP.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls, ocsp),
					new CertificateData("trust", rootCert.GetEncoded()));
			}

			public CertificatePathValidationException GetExpectedExceptionForRevoked(RevokedType type)
			{
				switch (type) {
					case RevokedType.CAwithCRL:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が失効しています。",
								new PkixCertPathValidatorException("Certificate revocation after Wed Sep 03 20:49:10 Z 2008, reason: aACompromise"));

					case RevokedType.CAwithOCSP:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が失効しています。",
								new PkixCertPathValidatorException("Certificate revocation after Wed Sep 03 20:49:10 Z 2008, reason: aACompromise"));

					case RevokedType.EndEntitywithCRL:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が失効しています。",
								new PkixCertPathValidatorException("Certificate revocation after Wed Sep 03 20:49:10 Z 2008, reason: superseded"));

					case RevokedType.EndEntitywithOCSP:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が失効しています。",
								new PkixCertPathValidatorException("Certificate revocation after Wed Sep 03 20:49:10 Z 2008, reason: superseded"));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}


			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForNotRevoked(DateTime utcDateTime)
			{
				var before = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);
				var future = utcDateTime.AddSeconds(100);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 100L, before, after, interPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 200L, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(interPair.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddCrlEntry(BigInteger.ValueOf(100L), future, CrlReason.Superseded);

							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				var interCRL = new X509V2CrlGenerator()
					.Configure(interCert, utcDateTime, utcDateTime.AddSeconds(100),
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddCrlEntry(BigInteger.ValueOf(200L), future, CrlReason.Superseded);

							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair.Private, "SHA256withECDSA");

				var end = endCert.GetEncoded();
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}


			public enum ExpiredType
			{
				CA,
				EndEntity,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForExpired(ExpiredType type, DateTime utcDateTime)
			{
				var before1 = utcDateTime.AddSeconds(-100);
				var before2 = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before1, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 100L, before1, after, interPair.Public,
						action: gen =>
						{
							if (type == ExpiredType.CA) {
								gen.SetNotAfter(before2);
							}
						})
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 200L, before1, after, endPair.Public,
						gen =>
						{
							if (type == ExpiredType.EndEntity) {
								gen.SetNotAfter(before2);
							}
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));

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
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}

			public CertificatePathValidationException GetExpectedExceptionForExpired(ExpiredType type)
			{
				switch (type) {
					case ExpiredType.CA:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が有効期間内にありません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Could not validate certificate: certificate expired on 20080903204820GMT+00:00")));

					case ExpiredType.EndEntity:
						return new CertificatePathValidationException(VerificationStatus.INVALID,
								"証明書が有効期間内にありません。",
								new PkixCertPathBuilderException("Certification path could not be validated.",
									new PkixCertPathValidatorException("Could not validate certificate: certificate expired on 20080903204820GMT+00:00")));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}


			public enum RevokationExpiredType
			{
				CA,
				EndEntity,
			}

			public (CertificateData, CertificatePathValidationData, CertificateData) GetTestDataForRevokationExpired(RevokationExpiredType type, DateTime utcDateTime)
			{
				var before1 = utcDateTime.AddSeconds(-100);
				var before2 = utcDateTime.AddSeconds(-50);
				var after = utcDateTime.AddSeconds(50);

				var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var rootCert = new X509V3CertificateGenerator()
					.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before1, after, rootPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name("CN=Test Intermediate CA Certificate"), rootCert, 1L, before1, after, interPair.Public)
					.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

				var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var endCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), interCert, 1L, before1, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(interPair.Private, "SHA256withECDSA");

				var rootCRL = new X509V2CrlGenerator()
					.Configure(rootCert, before1, type == RevokationExpiredType.CA ? before2 : after,
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(rootPair.Private, "SHA256withECDSA");

				var interCRL = new X509V2CrlGenerator()
					.Configure(interCert, before1, type == RevokationExpiredType.EndEntity ? before2 : after,
						gen =>
						{
							gen.AddCrlEntry(BigInteger.Two, utcDateTime, CrlReason.PrivilegeWithdrawn);
							gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));

						})
					.GenerateCrl(interPair.Private, "SHA256withECDSA");

				var end = endCert.GetEncoded();
				var certs = new[] { rootCert.GetEncoded(), interCert.GetEncoded() };
				var crls = new[] { rootCRL.GetEncoded(), interCRL.GetEncoded() };

				return (
					new CertificateData("unknown", end),
					new CertificatePathValidationData("unknown", certs, crls),
					new CertificateData("trust", rootCert.GetEncoded()));
			}

			public CertificatePathValidationException GetExpectedExceptionForRevokationExpired(RevokationExpiredType type)
			{
				switch (type) {
					case RevokationExpiredType.CA:
						return new CertificatePathValidationException(VerificationStatus.INDETERMINATE,
								"失効情報が発行されていません。",
								new PkixCertPathValidatorException("No CRLs found for issuer \"CN=Test Root CA Certificate\""));

					case RevokationExpiredType.EndEntity:
						return new CertificatePathValidationException(VerificationStatus.INDETERMINATE,
								"失効情報が発行されていません。",
								new PkixCertPathValidatorException("No CRLs found for issuer \"CN=Test Intermediate CA Certificate\""));

					default:
						throw new InvalidOperationException($"Invalid test type.");
				};
			}
		}

	}
}
