using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle.Asn1.HPKI;


namespace SignatureVerifier.TestUtilities
{
	internal class CertificatePathValidationDataGenerator
	{

		public (CertificateData, CertificatePathValidationData, CertificateData) GetTestData(DateTime utcDateTime,
			string targetCertId = "unknown-cert",
			int caLength = 2)
		{
			var before = utcDateTime.AddSeconds(-50);
			var after = utcDateTime.AddSeconds(50);

			var certList = new List<X509Certificate>();
			var crlList = new List<X509Crl>();
			var ocspList = new List<OcspResp>();

			var rootPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
			var rootCert = new X509V3CertificateGenerator()
				.ConfigureTestRootCA(new X509Name("CN=Test Root CA Certificate"), 1L, before, after, rootPair.Public)
				.GenerateCertificate(rootPair.Private, "SHA256withECDSA");

			certList.Add(rootCert);


			var parentPair = rootPair;
			var parentCert = rootCert;

			var interCaLength = caLength;
			foreach (var i in Enumerable.Range(100, caLength)) {
				--interCaLength;

				var interPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var interCert = new X509V3CertificateGenerator()
					.ConfigureTestCA(new X509Name($"CN=No.{i} Test Intermediate CA Certificate"), parentCert, (ulong)i, before, after, interPair.Public,
						pathLenConstraint: interCaLength)
					.GenerateCertificate(parentPair.Private, "SHA256withECDSA");
				certList.Add(interCert);

				if (i % 2 == 0) {
					var interCRL = new X509V2CrlGenerator()
						.Configure(parentCert, utcDateTime, utcDateTime.AddSeconds(100),
							gen =>
							{
								gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));
							})
						.GenerateCrl(parentPair.Private, "SHA256withECDSA");
					crlList.Add(interCRL);
				}
				else {
					var interOCSP = new BasicOcspRespGenerator(parentPair.Public)
					.Configure(
						gen =>
						{
							var id = new CertificateID(CertificateID.HashSha1, parentCert, interCert.SerialNumber);
							//gen.AddResponse(id, CertificateStatus.Good);
							gen.AddResponse(id, new RevokedStatus(utcDateTime, CrlReason.AACompromise)
								, utcDateTime, utcDateTime.AddSeconds(100), null);
						})
					.GenerateOcspResp(OCSPRespGenerator.Successful,
						"SHA256withECDSA", parentPair.Private, parentCert, utcDateTime);
					ocspList.Add(interOCSP);
				}

				parentPair = interPair;
				parentCert = interCert;
			}

			var endPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();

			ulong serialNumber;
			unchecked {
				serialNumber = (ulong)long.MaxValue + 1UL;
			}
			var endCert = new X509V3CertificateGenerator()
				.ConfigureEndEntity(new X509Name("CN=Test End Entity Certificate"), parentCert, serialNumber, before, after, endPair.Public,
					gen =>
					{
						gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));

						gen.AddExtension(X509Extensions.ExtendedKeyUsage, false,
							new ExtendedKeyUsage(
								KeyPurposeID.IdKPServerAuth,
								KeyPurposeID.IdKPIpsecUser,
								KeyPurposeID.IdKPTimeStamping
							));

						gen.AddExtension(X509Extensions.SubjectDirectoryAttributes, false,
							new SubjectDirectoryAttributes(new object[]{
								new AttributeX509(HPKIIdentifier.id_hcpki_at_healthcareactor,
									new DerSet(new DerSet(
										new HCActor(
											new CodedData(HPKIIdentifier.id_jhpki_cdata, null, new DerUtf8String("Medical Tester"))
										)
									))
								)
							}));

					})
				.GenerateCertificate(parentPair.Private, "SHA256withECDSA");

			var endCrl = new X509V2CrlGenerator()
				.Configure(parentCert, utcDateTime, utcDateTime.AddSeconds(100),
					gen =>
					{
						gen.AddCrlEntry(endCert.SerialNumber, after.AddSeconds(10), CrlReason.Superseded);
						gen.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(BigInteger.One));
					})
				.GenerateCrl(parentPair.Private, "SHA256withECDSA");
			crlList.Add(endCrl);

			//dummy cert
			foreach (var i in Enumerable.Range(500, 10)) {

				var dummyPair = GeneratorUtilities.GetKeyPairGenerator("ECDSA").GenerateECDSAKeyPair();
				var dummyCert = new X509V3CertificateGenerator()
					.ConfigureEndEntity(new X509Name($"CN=Test Dummy No.{i} End Entity Certificate"), parentCert, (ulong)i, before, after, endPair.Public,
						gen =>
						{
							gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature));
						})
					.GenerateCertificate(parentPair.Private, "SHA256withECDSA");
				certList.Add(dummyCert);
			}

			var end = endCert.GetEncoded();
			var certs = certList.Select(x => x.GetEncoded()).ToArray();
			var crls = crlList.Select(x => x.GetEncoded()).ToArray();
			var ocsps = ocspList.Select(x => x.GetEncoded()).ToArray();

			return (
				new CertificateData("test-ref1", targetCertId, end),
				new CertificatePathValidationData("test-ref2", certs, crls, ocsps),
				new CertificateData("trust", rootCert.GetEncoded())
				);
		}
	}
}

