﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SignatureVerifier.Verifiers.StructureVerifiers
{
	[TestFixture]
	internal class XmlSchemaVerifierTests
	{
		static IEnumerable<string> XmlValidCases()
		{
			yield return PrescriptionXml1;
			yield return DispensingXml1;
		}

		static IEnumerable<string> XmlInvalidCases()
		{
			yield return Properties.Resources_XmlSchema.XmlSchemaPrescriptionError;
			yield return Properties.Resources_XmlSchema.XmlSchemaDispensingError;
		}

		[Test(Description = "XMLスキーマ - VALID")]
		[TestCaseSource(nameof(XmlValidCases))]
		public void VerifySchema(string xml)
		{
			var doc = TestData.CreateXmlDocument(xml);
			var data = new SignedDocumentXml(doc, "XmlSchema.xml");

			var isValid = true;

			void handler(object sender, VerifiedEventArgs e)
			{
				isValid = false;
			}

			var config = new VerificationConfig();
			IStructureVerifier target = new XmlSchemaVerifier(config);
			target.VerifiedEvent += handler;

			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.True);
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.VALID));
				//Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				//Assert.That(result.Items.FirstOrDefault(m => m.Type == SignatureSourceType.Prescription && m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

		}

		[Test(Description = "XMLスキーマ - INVALID")]
		[TestCaseSource(nameof(XmlInvalidCases))]
		public void VerifySchema_Error(string xml)
		{
			var doc = TestData.CreateXmlDocument(xml);
			var data = new SignedDocumentXml(doc, "XmlSchemaError.xml");

			var isValid = true;

			void handler(object sender, VerifiedEventArgs e)
			{
				isValid = false;
			}

			var config = new VerificationConfig();
			IStructureVerifier target = new XmlSchemaVerifier(config);
			target.VerifiedEvent += handler;

			var result = target.Verify(data);

			target.VerifiedEvent -= handler;

			Assert.Multiple(() =>
			{
				Assert.That(isValid, Is.False);
				Assert.That(result.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(result.Items.FirstOrDefault(m => m.Source == "署名構造" && m.ItemName == "XMLスキーマ")?.Status, Is.EqualTo(VerificationStatus.INVALID));
			});

		}

		static readonly string PrescriptionXml1 = @"<?xml version=""1.0"" encoding=""UTF-8""?><Document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""Document"" xsi:noNamespaceSchemaLocation=""EP.xsd""><Prescription><PrescriptionManagement id=""PrescriptionManagement""><Version Value=""EPS1.0""/></PrescriptionManagement><PrescriptionDocument id=""PrescriptionDocument"">U0oxCjEsMSw4MDAwMDEwLDk5LOaknOiovOeUqOeXhemZogoyLDg1Mi04MTU0LOmVt+W0juecjOmVt+W0juW4ggozLDAzLTM3NzUtMzExMSwwMy0zNzc1LTMxMTIs44Gd44Gu5LuW6YCj57Wh5YWI77yQ77yRCjQsMiwwMSzlhoXnp5EKNSwwMDAwMDAwMDAwMDAwMDEs772y7728772s7727776M776e776b772zLOWMu+iAheOAgOS4iemDjgoxMSwwMDAwMDAwMDAwMDAwMDEs6Zu75Yem44CA5pyJ5Yq55LqU5LqMLO++g+++nu++ne+9vO+9riDvvpXvvbPvvbrvvbPvvbrvvp7vvoYKMTIsMgoxMywyMDExMDEwMQoxNCwxCjIxLDEKMjIsMzExMzA4NDIKMjMsMjAyMiwwMTAwMDEsMSwwMQoyNCwxMDAsMTAwCjI1LDEKMjcsMjkxMjM0NTYsMTIzNDU2NwoyOCwyOTEyMzQ1NiwxMjM0NTY3CjI5LDI5MTIzNDU2LDEyMzQ1NjcKMzAsOTkxMjM0NTYtMDEyMzQ1Njc4OTA5OTEyMzQ1Ni0wMTIzNDU2Nzg5MCw5OTEyMzQ1Ni0wMTIzNDU2Nzg5MDk5MTIzNDU2LTAxMjM0NTY3ODkwCjMxLDExMTEKNTEsMjAyMjA5MDcKNTIsMjAyMjEyMzEKNjIsMQo4MSwxLDEs5YKZ6ICD77yQ77yRCjgyLDEsMTAwMDAwMDAwMDAwMDAwMQoxMDEsMSwxLCwxNAoxMTEsMSwzLDEwMTExMDAwMDAwMDAwMDAs77yR5pel77yR5Zue5bCx5a+d5YmN44CA5pyN55SoLDEKMTgxLDEsMSws55So5rOV6KOc6Laz5oOF5aCx77yQ77yRLCwKMjAxLDEsMSwxLDIsNjIyNjg4MTAxLOODn+ODi+ODquODs+ODoeODq+ODiO+8r++8pOmMoO+8ku+8lc68772HLDIsMSzpjKAKMjExLDEsMSwxMDEKMjIxLDEsMSwxLjI1LDEsLCwsLCwsLAoyMzEsMSwxLDAsMCwwLDAKMjQxLDEsMSwxLDEKMjgxLDEsMSwxLCzolqzlk4Hoo5zotrPmg4XloLHvvJDvvJEsCjEwMSwyLDMsLDEKMTExLDIsMywxMDUwVzIwMDAwMDAwMDAwLOmBqeWunOOAgOacjeeUqCwxCjE4MSwyLDEsLOeUqOazleijnOi2s+aDheWgse+8kO+8kSwsCjIwMSwyLDEsMSwyLDY2MjY0MDY4MyzjgqLjg6vjg6Hjgr/ou5/oho/jgIDvvJDvvI7vvJHvvIUsMSwxLOacrAoyMTEsMiwxLDEwMQoyMjEsMiwxLDEuMjUsMSwsLCwsLCwsCjIzMSwyLDEsMCwwLDAsMAoyNDEsMiwxLDEsMQoyODEsMiwxLDEsLOiWrOWTgeijnOi2s+aDheWgse+8kO+8kSw=</PrescriptionDocument><PrescriptionSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""PrescriptionSign""><xs:SignedInfo Id=""id898cfb77-SignedInfo""><xs:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/><xs:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256""/><xs:Reference Id=""id29b12603-Reference1-Detached"" URI=""#PrescriptionDocument""><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U=
</xs:DigestValue></xs:Reference><xs:Reference Id=""id041cb21b-Reference2-KeyInfo"" URI=""#id6a875f31-KeyInfo""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=
</xs:DigestValue></xs:Reference><xs:Reference Id=""id8b2ae0fe-Reference3-SignedProperties"" Type=""http://uri.etsi.org/01903#SignedProperties"" URI=""#idc51bfd03-SignedProperties""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=
</xs:DigestValue></xs:Reference></xs:SignedInfo><xs:SignatureValue Id=""id00fac23c-SignatureValue"">CE2jLDBwEc1NJRaoOZ/PvThIR7EuNrg5U5OkhXiq7T+A6Erb2wv+lcbnZWfmtdem1BaKzcI9kk2j
YpG5nG9sIJMq5+AGjDszZ10mkshzg9L44FcgnB4TgVD32U6o361lqYXUgIPHpD8uINGNvMVjRdCP
q6207euvjLP35Wyf2Hcv9dmmWSiJJQ1y6Udz6EKxss7bSKxZUwjArgfoWW31LoNUFZwt4qwyIuB5
RfKsDa/bi3sj1d5BGRDVbmmC8EYzcwjvXlcRzBh5a4qRT549alcAiSu7cdhThQc4PN4yHiskn58d
O+Lax7yN4Pkn9vJeaGhtT6WF2pgFgX5GF2ojHA==
</xs:SignatureValue><xs:KeyInfo Id=""id6a875f31-KeyInfo""><xs:X509Data><xs:X509Certificate>MIIFgjCCBGqgAwIBAgICAV4wDQYJKoZIhvcNAQELBQAwZjELMAkGA1UEBhMCSlAxDjAMBgNVBAoM
BU1FRElTMRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQDDCZIUEtJLTAxLU1lZGlzU2ln
bkNBMi1mb3JOb25SZXB1ZGlhdGlvbjAeFw0yMjAyMDYxNTAwMDBaFw0yNzAyMDcxNDU5NTlaMGQx
CzAJBgNVBAYTAkpQMSIwIAYDVQQKDBlNRURJUyBVTklWRVJTSVRZIEhPU1BJVEFMMRwwGgYDVQQD
DBNTYW5qdXNoaSBLYWd1cmF6YWthMRMwEQYDVQQFEwpUZXN0MTE3MTIwMIIBIjANBgkqhkiG9w0B
AQEFAAOCAQ8AMIIBCgKCAQEAy6PVZxPcrXC3jW+teT7YLvl2+0lfHRbZWjf56eMn8mRUKKNsuPAu
tvSaTbRBL+E4S5HIzYR7dUtqqXnRg+xH1YPe2eaFAfzJFBNZggHTjYfV2wmX0kE/0sssjy5A1BUZ
4zRN1+oGantT8CkLJD4EsTZX1LCNJwUsBpkzN6kyhc4HddG9M3ISGa0dMtP31L1XcjCs/cP3xCJb
8Z5osEacccwVBaiL7CdpATEcro+xEJtYii0IibH9KJcrxOEbUkP8KWTzC2npwA+YauAmC6s8GfMr
wAEb0U8x+v58gUe4hVJS6PmW3QhnwVHFKy5Yp6zbDxB2jEomOp8iIvsSUXBBMwIDAQABo4ICOjCC
AjYwgcgGA1UdIwSBwDCBvYAUndYTUkXvvV54UshYjni8VB4/luahgaGkgZ4wgZsxCzAJBgNVBAYT
AkpQMS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFyZTE8MDoG
A1UECwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFsdWF0aW9u
MR0wGwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMoIBBDAdBgNVHQ4EFgQUwUIYMzPxAiX6Mwch
BNWKrcAL4UUwDgYDVR0PAQH/BAQDAgZAMHIGA1UdEQRrMGmkZzBlMQswCQYDVQQGEwJKUDEhMB8G
A1UECgwY5Yy755mC5oOF5aCx5aSn5a2m55eF6ZmiMR4wHAYDVQQDDBXnpZ7mpb3lnYLjgIDkuInl
jYHlm5sxEzARBgNVBAUTClRlc3QxMTcxMjAwOwYDVR0fBDQwMjAwoC6gLIYqaHR0cDovL2NlcnQu
bWVkaXMub3IuanAvc2lnbi9jcmwtc2lnbjIuY3JsMD0GA1UdCQQ2MDQwMgYGKIGFQgABMSgxJjAk
oCIxIKAMBgoqgwiGkQ8BBgEBohAMDk1lZGljYWwgRG9jdG9yMEoGA1UdIAEB/wRAMD4wPAYMKoMI
hpEPAQUBAQABMCwwKgYIKwYBBQUHAgEWHmh0dHA6Ly93d3cubWVkaXMub3IuanAvOF9ocGtpLzAN
BgkqhkiG9w0BAQsFAAOCAQEAmUx6UQDm6HboCK39k2/6qL9uAB9kSQtlyLxQ8Jpwdb7Q8iqNfyyo
ZXfMArjQpEcCQc63fsNoDItQfFjI8YJPMZExAVsPI1n/3NnelX00rxshPvZQvsryYntNUMykBbro
hlKx1hQJFCIETzXeBKrNJIHTvy+WF/3nAsIp6hU3mu2y9qzPhX0UViIlJQbGEhIj5JXmNYaVpYwR
DqOArUheAmbDevNNXKRsEfNICmtxotY1WlPdbneh3axHutxrzs5E4i+0XCr8m+C2Fz+bQoChiMTP
5R4USWe3R3Nt+dNZjbj1OvEnLEUT5tOfpZ0KzXDJZht5DOHLUhtV6+WuI+6Yqw==
</xs:X509Certificate></xs:X509Data></xs:KeyInfo><xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties Id=""id993b4fd6-QualifyingProperties"" Target=""#PrescriptionSign""><xa:SignedProperties Id=""idc51bfd03-SignedProperties""><xa:SignedSignatureProperties><xa:SigningTime>2022-09-07T08:08:01+00:00</xa:SigningTime><xa:SigningCertificateV2><xa:Cert><xa:CertDigest><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>NqeepkXkMTTnbn2s3W197fpOof0tj+Rd8JLII6BofmQ=
</xs:DigestValue></xa:CertDigest></xa:Cert></xa:SigningCertificateV2></xa:SignedSignatureProperties></xa:SignedProperties></xa:QualifyingProperties></xs:Object></xs:Signature></PrescriptionSign></Prescription></Document>";

		static readonly string DispensingXml1 = @"<?xml version=""1.0"" encoding=""UTF-8""?><Document xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""Document"" xsi:noNamespaceSchemaLocation=""ED.xsd"">
	<DispensingManagement id=""DispensingManagement"">
		<Version Value=""EPD1.0""/>
	</DispensingManagement>
	<Dispensing id=""Dispensing"">
		<ReferencePrescription>
			<PrescriptionManagement id=""PrescriptionManagement""><Version Value=""EPS1.0""/><PrescriptionId Value=""1439e0c5-7c1d-4512-861d-263de48417bf""/></PrescriptionManagement><PrescriptionDocument id=""PrescriptionDocument"">U0oxCjEsMSw4MDAwMDEwLDk5LOaknOiovOeUqOeXhemZogoyLDg1Mi04MTU0LOmVt+W0juecjOmVt+W0juW4ggozLDAzLTM3NzUtMzExMSwwMy0zNzc1LTMxMTIs44Gd44Gu5LuW6YCj57Wh5YWI77yQ77yRCjQsMiwwMSzlhoXnp5EKNSwwMDAwMDAwMDAwMDAwMDEs772y7728772s7727776M776e776b772zLOWMu+iAheOAgOS4iemDjgoxMSwwMDAwMDAwMDAwMDAwMDEs6Zu75Yem44CA5pyJ5Yq55LqU5LqMLO++g+++nu++ne+9vO+9riDvvpXvvbPvvbrvvbPvvbrvvp7vvoYKMTIsMgoxMywyMDExMDEwMQoxNCwxCjIxLDEKMjIsMzExMzA4NDIKMjMsMjAyMiwwMTAwMDEsMSwwMQoyNCwxMDAsMTAwCjI1LDEKMjcsMjkxMjM0NTYsMTIzNDU2NwoyOCwyOTEyMzQ1NiwxMjM0NTY3CjI5LDI5MTIzNDU2LDEyMzQ1NjcKMzAsOTkxMjM0NTYtMDEyMzQ1Njc4OTA5OTEyMzQ1Ni0wMTIzNDU2Nzg5MCw5OTEyMzQ1Ni0wMTIzNDU2Nzg5MDk5MTIzNDU2LTAxMjM0NTY3ODkwCjMxLDExMTEKNTEsMjAyMjA5MDcKNTIsMjAyMjEyMzEKNjIsMQo4MSwxLDEs5YKZ6ICD77yQ77yRCjgyLDEsMTAwMDAwMDAwMDAwMDAwMQoxMDEsMSwxLCwxNAoxMTEsMSwzLDEwMTExMDAwMDAwMDAwMDAs77yR5pel77yR5Zue5bCx5a+d5YmN44CA5pyN55SoLDEKMTgxLDEsMSws55So5rOV6KOc6Laz5oOF5aCx77yQ77yRLCwKMjAxLDEsMSwxLDIsNjIyNjg4MTAxLOODn+ODi+ODquODs+ODoeODq+ODiO+8r++8pOmMoO+8ku+8lc68772HLDIsMSzpjKAKMjExLDEsMSwxMDEKMjIxLDEsMSwxLjI1LDEsLCwsLCwsLAoyMzEsMSwxLDAsMCwwLDAKMjQxLDEsMSwxLDEKMjgxLDEsMSwxLCzolqzlk4Hoo5zotrPmg4XloLHvvJDvvJEsCjEwMSwyLDMsLDEKMTExLDIsMywxMDUwVzIwMDAwMDAwMDAwLOmBqeWunOOAgOacjeeUqCwxCjE4MSwyLDEsLOeUqOazleijnOi2s+aDheWgse+8kO+8kSwsCjIwMSwyLDEsMSwyLDY2MjY0MDY4MyzjgqLjg6vjg6Hjgr/ou5/oho/jgIDvvJDvvI7vvJHvvIUsMSwxLOacrAoyMTEsMiwxLDEwMQoyMjEsMiwxLDEuMjUsMSwsLCwsLCwsCjIzMSwyLDEsMCwwLDAsMAoyNDEsMiwxLDEsMQoyODEsMiwxLDEsLOiWrOWTgeijnOi2s+aDheWgse+8kO+8kSw=</PrescriptionDocument><AdditionalPrescriptionInformation id=""AdditionalPrescriptionInformation""><ConversionPharmaceuticalCode><ConversionREcode>201,1,1,1,2,622688101,ミニリンメルトＯＤ錠２５μｇ,2,1,錠</ConversionREcode><ConversionYJcode>201,1,1,1,4,2419001F4022,ミニリンメルトＯＤ錠２５μｇ,2,1,錠</ConversionYJcode></ConversionPharmaceuticalCode><ConversionPharmaceuticalCode><ConversionREcode>201,2,1,1,2,662640683,アルメタ軟膏　０．１％,1,1,本</ConversionREcode><ConversionYJcode>201,2,1,1,4,2646727M1022,アルメタ軟膏,1,1,本</ConversionYJcode></ConversionPharmaceuticalCode></AdditionalPrescriptionInformation><PrescriptionSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""PrescriptionSign""><xs:SignedInfo Id=""id898cfb77-SignedInfo""><xs:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/><xs:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256""/><xs:Reference Id=""id29b12603-Reference1-Detached"" URI=""#PrescriptionDocument""><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>d2AViR1mbbTGkOUKs3/DNI66RLps3c8tE/UDoled78U=
</xs:DigestValue></xs:Reference><xs:Reference Id=""id041cb21b-Reference2-KeyInfo"" URI=""#id6a875f31-KeyInfo""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>bI4jGyBqLZB28i/cH8njp05705/ji/sUPptLX/M2ycQ=
</xs:DigestValue></xs:Reference><xs:Reference Id=""id8b2ae0fe-Reference3-SignedProperties"" Type=""http://uri.etsi.org/01903#SignedProperties"" URI=""#idc51bfd03-SignedProperties""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>8XgGgHpPS/3nAQTc6cVSGHI5ObMWjQ8l3oHpyQkF4jQ=
</xs:DigestValue></xs:Reference></xs:SignedInfo><xs:SignatureValue Id=""id00fac23c-SignatureValue"">CE2jLDBwEc1NJRaoOZ/PvThIR7EuNrg5U5OkhXiq7T+A6Erb2wv+lcbnZWfmtdem1BaKzcI9kk2j
YpG5nG9sIJMq5+AGjDszZ10mkshzg9L44FcgnB4TgVD32U6o361lqYXUgIPHpD8uINGNvMVjRdCP
q6207euvjLP35Wyf2Hcv9dmmWSiJJQ1y6Udz6EKxss7bSKxZUwjArgfoWW31LoNUFZwt4qwyIuB5
RfKsDa/bi3sj1d5BGRDVbmmC8EYzcwjvXlcRzBh5a4qRT549alcAiSu7cdhThQc4PN4yHiskn58d
O+Lax7yN4Pkn9vJeaGhtT6WF2pgFgX5GF2ojHA==
</xs:SignatureValue><xs:KeyInfo Id=""id6a875f31-KeyInfo""><xs:X509Data><xs:X509Certificate>MIIFgjCCBGqgAwIBAgICAV4wDQYJKoZIhvcNAQELBQAwZjELMAkGA1UEBhMCSlAxDjAMBgNVBAoM
BU1FRElTMRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQDDCZIUEtJLTAxLU1lZGlzU2ln
bkNBMi1mb3JOb25SZXB1ZGlhdGlvbjAeFw0yMjAyMDYxNTAwMDBaFw0yNzAyMDcxNDU5NTlaMGQx
CzAJBgNVBAYTAkpQMSIwIAYDVQQKDBlNRURJUyBVTklWRVJTSVRZIEhPU1BJVEFMMRwwGgYDVQQD
DBNTYW5qdXNoaSBLYWd1cmF6YWthMRMwEQYDVQQFEwpUZXN0MTE3MTIwMIIBIjANBgkqhkiG9w0B
AQEFAAOCAQ8AMIIBCgKCAQEAy6PVZxPcrXC3jW+teT7YLvl2+0lfHRbZWjf56eMn8mRUKKNsuPAu
tvSaTbRBL+E4S5HIzYR7dUtqqXnRg+xH1YPe2eaFAfzJFBNZggHTjYfV2wmX0kE/0sssjy5A1BUZ
4zRN1+oGantT8CkLJD4EsTZX1LCNJwUsBpkzN6kyhc4HddG9M3ISGa0dMtP31L1XcjCs/cP3xCJb
8Z5osEacccwVBaiL7CdpATEcro+xEJtYii0IibH9KJcrxOEbUkP8KWTzC2npwA+YauAmC6s8GfMr
wAEb0U8x+v58gUe4hVJS6PmW3QhnwVHFKy5Yp6zbDxB2jEomOp8iIvsSUXBBMwIDAQABo4ICOjCC
AjYwgcgGA1UdIwSBwDCBvYAUndYTUkXvvV54UshYjni8VB4/luahgaGkgZ4wgZsxCzAJBgNVBAYT
AkpQMS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFyZTE8MDoG
A1UECwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFsdWF0aW9u
MR0wGwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMoIBBDAdBgNVHQ4EFgQUwUIYMzPxAiX6Mwch
BNWKrcAL4UUwDgYDVR0PAQH/BAQDAgZAMHIGA1UdEQRrMGmkZzBlMQswCQYDVQQGEwJKUDEhMB8G
A1UECgwY5Yy755mC5oOF5aCx5aSn5a2m55eF6ZmiMR4wHAYDVQQDDBXnpZ7mpb3lnYLjgIDkuInl
jYHlm5sxEzARBgNVBAUTClRlc3QxMTcxMjAwOwYDVR0fBDQwMjAwoC6gLIYqaHR0cDovL2NlcnQu
bWVkaXMub3IuanAvc2lnbi9jcmwtc2lnbjIuY3JsMD0GA1UdCQQ2MDQwMgYGKIGFQgABMSgxJjAk
oCIxIKAMBgoqgwiGkQ8BBgEBohAMDk1lZGljYWwgRG9jdG9yMEoGA1UdIAEB/wRAMD4wPAYMKoMI
hpEPAQUBAQABMCwwKgYIKwYBBQUHAgEWHmh0dHA6Ly93d3cubWVkaXMub3IuanAvOF9ocGtpLzAN
BgkqhkiG9w0BAQsFAAOCAQEAmUx6UQDm6HboCK39k2/6qL9uAB9kSQtlyLxQ8Jpwdb7Q8iqNfyyo
ZXfMArjQpEcCQc63fsNoDItQfFjI8YJPMZExAVsPI1n/3NnelX00rxshPvZQvsryYntNUMykBbro
hlKx1hQJFCIETzXeBKrNJIHTvy+WF/3nAsIp6hU3mu2y9qzPhX0UViIlJQbGEhIj5JXmNYaVpYwR
DqOArUheAmbDevNNXKRsEfNICmtxotY1WlPdbneh3axHutxrzs5E4i+0XCr8m+C2Fz+bQoChiMTP
5R4USWe3R3Nt+dNZjbj1OvEnLEUT5tOfpZ0KzXDJZht5DOHLUhtV6+WuI+6Yqw==
</xs:X509Certificate></xs:X509Data></xs:KeyInfo><xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties Id=""id993b4fd6-QualifyingProperties"" Target=""#PrescriptionSign""><xa:SignedProperties Id=""idc51bfd03-SignedProperties""><xa:SignedSignatureProperties><xa:SigningTime>2022-09-07T08:08:01+00:00</xa:SigningTime><xa:SigningCertificateV2><xa:Cert><xa:CertDigest><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>NqeepkXkMTTnbn2s3W197fpOof0tj+Rd8JLII6BofmQ=
</xs:DigestValue></xa:CertDigest></xa:Cert></xa:SigningCertificateV2></xa:SignedSignatureProperties></xa:SignedProperties><xa:UnsignedProperties><xa:UnsignedSignatureProperties><xa:SignatureTimeStamp Id=""idbc9e4f38""><xa:EncapsulatedTimeStamp>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
BMlkLuXc5/v3F7n76Jxhl4sdT84=
</xa:EncapsulatedTimeStamp></xa:SignatureTimeStamp><xa141:TimeStampValidationData xmlns:xa141=""http://uri.etsi.org/01903/v1.4.1#"" Id=""id7c14a0c8"" URI=""#idbc9e4f38""><CertificateValues xmlns=""http://uri.etsi.org/01903/v1.3.2#"" Id=""id389cb7f2""><xa:EncapsulatedX509Certificate>MIIDqTCCApGgAwIBAgIBJTANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJKUDEcMBoGA1UEChMT
U2FtcGxlIE9yZ2FuaXphdGlvbjESMBAGA1UECxMJU2FtcGxlIENBMSQwIgYDVQQDExtUZXN0IENB
IGZvciBUaW1lU3RhbXBTZXJ2ZXIwHhcNMTcwNjIxMTAyODEyWhcNMzMxMjMxMTUwMDAwWjBXMQsw
CQYDVQQGEwJKUDENMAsGA1UEChMETURJUzEdMBsGA1UECxMURmluYW5jaWFsIERlcGFydG1lbnQx
GjAYBgNVBAMTEVRpbWVTdGFtcFNlcnZlcjAxMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC
AQEA2JNfR4b84cuqx+Obf2FX5lFK45RoxqvCrjvuNT5vbWZGinnmPqIwgZiiOw7qV2GZCN6l3VuI
87Ds+NE6Fd9APTo7KOCUXnn+Q4IMp11s+++dnq7/n8UDEH/Y/0w4Hgco4WT9r5QWobNCdvmkWpuv
p5wm78msw+nGMThROIMQN90uyuId6aSWEZwDcqLRoaIOaY4KXW52w5WOfNjZxw9znHtFTW5QWVOP
2DfS+ET+iwwT8kkOxHj5oYlxThSgBAkA536RtKmZHCcRsdaliNPFBKaiQWP7CrjSGuab/l7e3BoI
QA3cPmKCSym45amHgBYZq0WaT/BXNkCM2QXhdpPq6QIDAQABo3IwcDAdBgNVHQ4EFgQUwhY9pQih
RxKqVHBuco0PGQvHJtUwHwYDVR0jBBgwFoAUPtzxR5kBmsO6I2N+kGCWAlXEuOIwCQYDVR0TBAIw
ADALBgNVHQ8EBAMCBsAwFgYDVR0lAQH/BAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcNAQELBQADggEB
AGw6XGu2+k2EqUWx7aSVf8E8c+vMdG3lCuTbPz7Et4WRvUT1NZRbLEBpf8GRC4X+SOs489j+rQ0g
jgi4u4Q1aI+NrN5bf1q4OaRkbPwuiWpxYdwmCjaswmZGY83NXrVjqRE1jYnH6cl0gc8O6z3JpxvE
9qgODyHqsTzwy4sIS8V/gJVXyPPLh4wUtfR3aTPvjZgMkCUGSLz4wBdEkwtZKfVGnAmn/BGVlMsp
sbuRBOaottZCuOnkOy4V9Y98cG2P7mrYczy+LLoKZ4125yRTAGXCCn1svmPjwMufmaAcY4bHpKpK
ZL8+r21bUo/DjhYSRIIEeARFIgTv0m1KnP5AGE0=
</xa:EncapsulatedX509Certificate><xa:EncapsulatedX509Certificate>MIIEFzCCAv+gAwIBAgIJAIK9bBWUV3OZMA0GCSqGSIb3DQEBCwUAMGUxCzAJBgNVBAYTAkpQMRww
GgYDVQQKExNTYW1wbGUgT3JnYW5pemF0aW9uMRIwEAYDVQQLEwlTYW1wbGUgQ0ExJDAiBgNVBAMT
G1Rlc3QgQ0EgZm9yIFRpbWVTdGFtcFNlcnZlcjAeFw0xNzA2MjExMDI4MDZaFw0yNzA2MjkxMDI4
MDZaMGUxCzAJBgNVBAYTAkpQMRwwGgYDVQQKExNTYW1wbGUgT3JnYW5pemF0aW9uMRIwEAYDVQQL
EwlTYW1wbGUgQ0ExJDAiBgNVBAMTG1Rlc3QgQ0EgZm9yIFRpbWVTdGFtcFNlcnZlcjCCASIwDQYJ
KoZIhvcNAQEBBQADggEPADCCAQoCggEBAM7FtYwz28X2bandNM4hJk3zsObkM/apz7eyFl7Jbd6O
v3ZmDlrjv9ADHaOMcMi3VwqGdcHg2GLbJ+ev2iI/7oPDRWA4CbUZq5XSsHg/aXSWfDzNKV2fMbjq
j8Y0AZx9HKDonCRTimRhhyqvk4wXj1FqzNxbVyMlJkr6eddOXZ/s4inxt/F7HZ32Lgdiy62JA8Ji
yUTgwXvOHmtyPjTGvVvBurkWfKgklxHDPryupM7VMsA0Ii32QZWAaTBHfw68bCotQJTmEl9qgAIs
ZUgLj0Q7iF+r67zfVkcMKecVkPPNtyvd1MaAlmJb+w7rrKwAVkBqAkLwMjaQ1MMNJ+b+eYcCAwEA
AaOByTCBxjAdBgNVHQ4EFgQUPtzxR5kBmsO6I2N+kGCWAlXEuOIwgZcGA1UdIwSBjzCBjIAUPtzx
R5kBmsO6I2N+kGCWAlXEuOKhaaRnMGUxCzAJBgNVBAYTAkpQMRwwGgYDVQQKExNTYW1wbGUgT3Jn
YW5pemF0aW9uMRIwEAYDVQQLEwlTYW1wbGUgQ0ExJDAiBgNVBAMTG1Rlc3QgQ0EgZm9yIFRpbWVT
dGFtcFNlcnZlcoIJAIK9bBWUV3OZMAsGA1UdDwQEAwIBBjANBgkqhkiG9w0BAQsFAAOCAQEAJH9B
QOXJQGP91iKWHWNYWAx7pPGDYIw2HV9HvuWqfz9PDTq7cr2CDl3WltwZzp0xe7vz9bCggmRrodoN
GRUDG3aYXHgWfGtZpviJTKa0k65eCCFWIBz9K69sLkLtHUs/GqpcVon6XIKCWv2UcGhmFKT6slx7
3f3V4+MuDDsSUc3k2kNMqBBo43fy7Kv8hY6z6XDcjX7VQAlgeS2uddLb84eMM8ekOVSfZj4CSWGs
+J6WHdPj/48Y5hv3Y1dgLf4qIdv187iuSdDB435KwFUuHjewf+jAzbNVMzdzz3pjG0M3ZIXplE2B
Q0vYK5wir7YCpFqEj1ffA/emGE2FoWJNSA==
</xa:EncapsulatedX509Certificate></CertificateValues><xa:RevocationValues Id=""id28114366""><xa:CRLValues><xa:EncapsulatedCRLValue>MIIBvzCBqAIBATANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJKUDEcMBoGA1UEChMTU2FtcGxl
IE9yZ2FuaXphdGlvbjESMBAGA1UECxMJU2FtcGxlIENBMSQwIgYDVQQDExtUZXN0IENBIGZvciBU
aW1lU3RhbXBTZXJ2ZXIXDTE3MDYyMTEwMjgyN1oXDTI3MDYxOTEwMjgyN1qgDzANMAsGA1UdFAQE
AgIBADANBgkqhkiG9w0BAQsFAAOCAQEADuk19hLbnRb5UibKKx38CNVdaDSPM8vkX08t9VvDc2bM
Lq2fJ4sd3hsbciSJYFYxG2q95hYv8WUOQ2/yuMQHc69X+BE8Rs2TqcjZrALXy9gw9uIthHDPsRNe
oNbqCrNALfPnMldgJI2jX2UnFamEaUZn7T4PQ0PspAKkQcAxdeh0ofUH1VPkFcdJvgIZdYuZqzK8
LxLkFsHFHjl3NVCHUXm8DWJx2SVGtJdTqtAp/1dO4MtggXZAzFZ59geR1IA7dGiz1zQsvqK5S3s2
FLyUbIu6QAzeda46/3NJPxnfwk93RRdKRHa+kr3EQgz7UVw8JZ7FOzELVuoqLTj6RHMNbA==
</xa:EncapsulatedCRLValue></xa:CRLValues></xa:RevocationValues></xa141:TimeStampValidationData><xa:CertificateValues Id=""id83f622cc""><xa:EncapsulatedX509Certificate>MIIF3jCCBMagAwIBAgIBBDANBgkqhkiG9w0BAQsFADCBmzELMAkGA1UEBhMCSlAxLzAtBgNVBAoM
Jk1pbmlzdHJ5IG9mIEhlYWx0aCwgTGFib3VyIGFuZCBXZWxmYXJlMTwwOgYDVQQLDDNEaXJlY3Rv
ci1HZW5lcmFsIGZvciBQb2xpY3kgUGxhbm5pbmcgYW5kIEV2YWx1YXRpb24xHTAbBgNVBAsMFE1I
TFcgSFBLSSBSb290IENBIFYyMB4XDTE1MDQyMDA0NDAzMFoXDTM1MDQxOTE0NTk1OVowZjELMAkG
A1UEBhMCSlAxDjAMBgNVBAoMBU1FRElTMRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQD
DCZIUEtJLTAxLU1lZGlzU2lnbkNBMi1mb3JOb25SZXB1ZGlhdGlvbjCCASIwDQYJKoZIhvcNAQEB
BQADggEPADCCAQoCggEBAKyqYyde+agCP6b8fmO5qFSMx13Eg+pHcVJmXO1FMo1QS1tYlwBxXOSj
EHXO3bXghEbpoKBMx1hpLffPACSWwL0gqv6JijgVFLCAkEgn8K0ImfVzc3eZC1D+cE7S/aRP/Z0P
PgjQsG65hA7WYJzGHzBRgFqTSGZ9dJ3k9oBr0T9BH3gzllQRJWvP1dAQtEIkHlQHDm5gH/q644QU
FAZXd2Mc8+PhuT7Ho9yNlIYXmdtmDDJ93qyVpn96LjmzKg2fV1jKBbTROZxUvhkmi0GW6toIiEfS
tgE1gPsuszDwmUaz/uzUAi2HSkmNiI3Zc3Mphinq8C2LmfgAPomXEbE2bbsCAwEAAaOCAl8wggJb
MIHIBgNVHSMEgcAwgb2AFAGlYw7g0lH2E6oVARUoidZBmDN/oYGhpIGeMIGbMQswCQYDVQQGEwJK
UDEvMC0GA1UECgwmTWluaXN0cnkgb2YgSGVhbHRoLCBMYWJvdXIgYW5kIFdlbGZhcmUxPDA6BgNV
BAsMM0RpcmVjdG9yLUdlbmVyYWwgZm9yIFBvbGljeSBQbGFubmluZyBhbmQgRXZhbHVhdGlvbjEd
MBsGA1UECwwUTUhMVyBIUEtJIFJvb3QgQ0EgVjKCAQEwHQYDVR0OBBYEFJ3WE1JF771eeFLIWI54
vFQeP5bmMA8GA1UdEwEB/wQFMAMBAf8wTgYDVR0gAQH/BEQwQjBABgwqgwiGkQ8BBQEBAwEwMDAu
BggrBgEFBQcCARYiaHR0cDovL2hwa2kubWhsdy5nby5qcC9yZXBvc2l0b3J5LzAOBgNVHQ8BAf8E
BAMCAQYwgf0GA1UdHwSB9TCB8jCBtqCBs6CBsKSBrTCBqjELMAkGA1UEBhMCSlAxLzAtBgNVBAoM
Jk1pbmlzdHJ5IG9mIEhlYWx0aCwgTGFib3VyIGFuZCBXZWxmYXJlMTwwOgYDVQQLDDNEaXJlY3Rv
ci1HZW5lcmFsIGZvciBQb2xpY3kgUGxhbm5pbmcgYW5kIEV2YWx1YXRpb24xHTAbBgNVBAsMFE1I
TFcgSFBLSSBSb290IENBIFYyMQ0wCwYDVQQDDARTQVJMMDegNaAzhjFodHRwOi8vaHBraS5taGx3
LmdvLmpwL3JlcG9zaXRvcnkvcmxpc3Qvc2FybDIuY3JsMA0GCSqGSIb3DQEBCwUAA4IBAQB4VwcY
IAIQIV7ND9IOMDc7Po+uG7Y97UFJpKvPbXbrDwT8BzF80P+7msK72WiNCbek9cWsfq8snsef32Vc
Ej9tmEHsvyaxOLA2aT+nTlhCv+HBtA5OZUxcwLaeCbOaiEpcxCYFhK7suz+ocIKxk+h0h4HYPHyT
5pYO/eQtADGQV13v9BY5ptj8S0yYbPBtvExSsr1NXm44kFsg8mN/zoolPvjqneErQfgya+6n87fv
CSnl5rrQ0Y/ggsiOj9nyUoAl55121X/8vXHRQtRYyp0tQs4ypMNubQIQ/mYAs+kVscY+nBDJBqHV
VOpjNPlBUPnK3/VtWTvlmmcpPH7bOHok
</xa:EncapsulatedX509Certificate><xa:EncapsulatedX509Certificate>MIIGVjCCBT6gAwIBAgIBATANBgkqhkiG9w0BAQsFADCBmzELMAkGA1UEBhMCSlAxLzAtBgNVBAoM
Jk1pbmlzdHJ5IG9mIEhlYWx0aCwgTGFib3VyIGFuZCBXZWxmYXJlMTwwOgYDVQQLDDNEaXJlY3Rv
ci1HZW5lcmFsIGZvciBQb2xpY3kgUGxhbm5pbmcgYW5kIEV2YWx1YXRpb24xHTAbBgNVBAsMFE1I
TFcgSFBLSSBSb290IENBIFYyMB4XDTE0MDExNzA2MDgwOFoXDTQ0MDExNjE0NTk1OVowgZsxCzAJ
BgNVBAYTAkpQMS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFy
ZTE8MDoGA1UECwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFs
dWF0aW9uMR0wGwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMjCCASIwDQYJKoZIhvcNAQEBBQAD
ggEPADCCAQoCggEBAK1UIokjCUGfaGRc5KVVwxw+HAEhGQatKHUNUXAg+Xbk9FGyTx6eZIvKAMnr
AsXw+nO6jJUhpI8HnZPjpAm02hKV8HngzEPO1OfRWBtyjUfHLygKTBVRIOoZixkBTIn38NLF5fUZ
xt9hjzZlLmUaoCncVJCrAB2miNDpagE/1min6liMVO2phPqkGfCFRnbQYTeacuQmIAY8FoqeDCWg
iWQsdlpdVxGNWyG2P/ToNSvzst0gG6vtYwROTvHw/UKuaFsQpVK4bifpjhsT47FCorM0YmQIPEI7
zbGCdmMLszTR/v0cBC6Xn5c3MWx9rsI2QUSCsj5bu99O1lfMKIaMUtECAwEAAaOCAqEwggKdMIHI
BgNVHSMEgcAwgb2AFAGlYw7g0lH2E6oVARUoidZBmDN/oYGhpIGeMIGbMQswCQYDVQQGEwJKUDEv
MC0GA1UECgwmTWluaXN0cnkgb2YgSGVhbHRoLCBMYWJvdXIgYW5kIFdlbGZhcmUxPDA6BgNVBAsM
M0RpcmVjdG9yLUdlbmVyYWwgZm9yIFBvbGljeSBQbGFubmluZyBhbmQgRXZhbHVhdGlvbjEdMBsG
A1UECwwUTUhMVyBIUEtJIFJvb3QgQ0EgVjKCAQEwHQYDVR0OBBYEFAGlYw7g0lH2E6oVARUoidZB
mDN/MA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0PAQH/BAQDAgEGMIGPBgNVHREEgYcwgYSkgYEwfzEL
MAkGA1UEBhMCSlAxGDAWBgNVBAoMD+WOmueUn+WKtOWDjeecgTEYMBYGA1UECwwP5pS/562W57Wx
5ous5a6YMTwwOgYDVQQLDDPljprnlJ/lirTlg43nnIHvvKjvvLDvvKvvvKnjg6vjg7zjg4joqo3o
qLzlsYDvvLbvvJIwgf0GA1UdHwSB9TCB8jCBtqCBs6CBsKSBrTCBqjELMAkGA1UEBhMCSlAxLzAt
BgNVBAoMJk1pbmlzdHJ5IG9mIEhlYWx0aCwgTGFib3VyIGFuZCBXZWxmYXJlMTwwOgYDVQQLDDNE
aXJlY3Rvci1HZW5lcmFsIGZvciBQb2xpY3kgUGxhbm5pbmcgYW5kIEV2YWx1YXRpb24xHTAbBgNV
BAsMFE1ITFcgSFBLSSBSb290IENBIFYyMQ0wCwYDVQQDDARSQVJMMDegNaAzhjFodHRwOi8vaHBr
aS5taGx3LmdvLmpwL3JlcG9zaXRvcnkvcmxpc3QvcmFybDIuY3JsMA0GCSqGSIb3DQEBCwUAA4IB
AQCUIQjgY/LtTZwX+1Tqt4RDJCyleIjsQ5FypvHhT6CZxlKjyuqyU+StWGXOPK2JYHVX0mjbsz80
i8rXY1qHqPQinkpzlEmL7O9CQpW1M+kxOEMz4IA9WTRs3k7tNL3ubOVJkSicMmpvMOUnYRV+0ERz
WLdgo/wWhkd/kaloAGIW3DiXUrIyEsu0Ok40DagWnDGFwlBpCtSlq+ChLA1E9m+9IoE1Bbb32xdG
ssr9b4E10K4//QZ3kQN7BaayMa6dixVESGkgVgMTZq7696dqmfPr85C3jzsEttu6rgPGRmuBhmCB
XUK+xjzNCu0AF6pOPfgy9Qo/mMNvY4RTijqBVzuA
</xa:EncapsulatedX509Certificate></xa:CertificateValues><xa:RevocationValues Id=""idf6a05a29""><xa:CRLValues><xa:EncapsulatedCRLValue>MIIGiTCCBXECAQEwDQYJKoZIhvcNAQELBQAwZjELMAkGA1UEBhMCSlAxDjAMBgNVBAoMBU1FRElT
MRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQDDCZIUEtJLTAxLU1lZGlzU2lnbkNBMi1m
b3JOb25SZXB1ZGlhdGlvbhcNMjIwOTA2MTgwMDAwWhcNMjIwOTEwMTgwMDAwWjCCA/cwIQICAJoX
DTE3MDkyMDAzMTcyM1owDDAKBgNVHRUEAwoBBTAhAgIA3RcNMTkwMTE3MDExNDU3WjAMMAoGA1Ud
FQQDCgEFMCECAgDkFw0xOTAzMTUwNDIzMDhaMAwwCgYDVR0VBAMKAQUwIQICAPMXDTE5MDgwODEw
MTM1OFowDDAKBgNVHRUEAwoBBTAhAgIA9xcNMTkwODI2MDIxMDEwWjAMMAoGA1UdFQQDCgEFMCEC
AgEDFw0xOTExMTgwMTI3NTdaMAwwCgYDVR0VBAMKAQUwIQICAQkXDTIyMDYxNzA0MzUwNFowDDAK
BgNVHRUEAwoBBTAhAgIBChcNMjIwNjE3MDQzNTA1WjAMMAoGA1UdFQQDCgEFMCECAgEVFw0yMDA0
MTMwNzQ0NDFaMAwwCgYDVR0VBAMKAQUwIQICASAXDTIwMDYyMjA3NTY0NlowDDAKBgNVHRUEAwoB
BTAhAgIBKBcNMjAxMDI2MDQ1MjM1WjAMMAoGA1UdFQQDCgEFMCECAgE0Fw0yMTAyMTYwODA3MDJa
MAwwCgYDVR0VBAMKAQUwIQICATUXDTIxMDIxNjA4MDcyMFowDDAKBgNVHRUEAwoBBTAhAgIBNhcN
MjEwMjE2MDgwNzMxWjAMMAoGA1UdFQQDCgEFMCECAgE5Fw0yMTA0MTMwNjE5MjFaMAwwCgYDVR0V
BAMKAQUwIQICAVIXDTIxMTEyOTA2MjAzMFowDDAKBgNVHRUEAwoBBTAhAgIBUxcNMjExMTI5MDYy
MDUyWjAMMAoGA1UdFQQDCgEFMCECAgFUFw0yMTExMjkwNjIxMTVaMAwwCgYDVR0VBAMKAQUwIQIC
AVUXDTIxMTEyOTA2MjEzOFowDDAKBgNVHRUEAwoBBTAhAgIBVhcNMjExMTI5MDYyMTU0WjAMMAoG
A1UdFQQDCgEFMCECAgFbFw0yMTEyMTUwNDQ1MDVaMAwwCgYDVR0VBAMKAQUwIQICAV8XDTIyMDIw
NDAyMjMxMFowDDAKBgNVHRUEAwoBBTAhAgIBYhcNMjIwMjA0MDIyMzI4WjAMMAoGA1UdFQQDCgEF
MCECAgFlFw0yMjAyMDQwMjIzNDFaMAwwCgYDVR0VBAMKAQUwIQICAYwXDTIyMDgwNDAxMzU1OVow
DDAKBgNVHRUEAwoBBTAhAgIBjxcNMjIwODE4MDUxMzM2WjAMMAoGA1UdFQQDCgEFMCECAgGSFw0y
MjA4MTgwMjU0MzVaMAwwCgYDVR0VBAMKAQUwIQICAZUXDTIyMDgyNDA1MTgxOVowDDAKBgNVHRUE
AwoBBTAhAgIBmBcNMjIwODI0MDUxODM3WjAMMAoGA1UdFQQDCgEFoIHbMIHYMIHIBgNVHSMEgcAw
gb2AFJ3WE1JF771eeFLIWI54vFQeP5bmoYGhpIGeMIGbMQswCQYDVQQGEwJKUDEvMC0GA1UECgwm
TWluaXN0cnkgb2YgSGVhbHRoLCBMYWJvdXIgYW5kIFdlbGZhcmUxPDA6BgNVBAsMM0RpcmVjdG9y
LUdlbmVyYWwgZm9yIFBvbGljeSBQbGFubmluZyBhbmQgRXZhbHVhdGlvbjEdMBsGA1UECwwUTUhM
VyBIUEtJIFJvb3QgQ0EgVjKCAQQwCwYDVR0UBAQCAgW+MA0GCSqGSIb3DQEBCwUAA4IBAQCWYk7Q
eWF6zhTm2kGYR0obgxt/QlXYj0Xft/Gowsjt0pyyorsY6hojBTmEY21fTQlpVjMt19VSGf3dYl7c
cQc6gg8dOyPGBdZGN4plRitG6d4dykB7dNrCM5jGq8W/SEpBhpaLTDtWVK2bS9oLcFR+X2yEABiX
0zg+eW1oQBemy0rVTE/pUwbZ5zUvA2M9ruxIyVkc1HgmdZLQUE/aWisZuH4xr/I7w47UsjMUysDZ
4t/Hl9HzqpLRb3yHjQ0G1FMI+09n0gB+Ta6D8WHEzBY0c3Fyd7nn8H/gv9ZvrIZKJtJdfh2isOji
VfvFEZYUb7oeP/Uj5c+SYIIuGzYeqMJo
</xa:EncapsulatedCRLValue><xa:EncapsulatedCRLValue>MIIDkDCCAngCAQEwDQYJKoZIhvcNAQELBQAwgZsxCzAJBgNVBAYTAkpQMS8wLQYDVQQKDCZNaW5p
c3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFyZTE8MDoGA1UECwwzRGlyZWN0b3ItR2Vu
ZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFsdWF0aW9uMR0wGwYDVQQLDBRNSExXIEhQ
S0kgUm9vdCBDQSBWMhcNMjIwOTA1MTkwMDAwWhcNMjIwOTA5MTkwMDAwWqCCAaYwggGiMIHIBgNV
HSMEgcAwgb2AFAGlYw7g0lH2E6oVARUoidZBmDN/oYGhpIGeMIGbMQswCQYDVQQGEwJKUDEvMC0G
A1UECgwmTWluaXN0cnkgb2YgSGVhbHRoLCBMYWJvdXIgYW5kIFdlbGZhcmUxPDA6BgNVBAsMM0Rp
cmVjdG9yLUdlbmVyYWwgZm9yIFBvbGljeSBQbGFubmluZyBhbmQgRXZhbHVhdGlvbjEdMBsGA1UE
CwwUTUhMVyBIUEtJIFJvb3QgQ0EgVjKCAQEwCwYDVR0UBAQCAgzFMIHHBgNVHRwBAf8Egbwwgbmg
gbOggbCkga0wgaoxCzAJBgNVBAYTAkpQMS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExh
Ym91ciBhbmQgV2VsZmFyZTE8MDoGA1UECwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBs
YW5uaW5nIGFuZCBFdmFsdWF0aW9uMR0wGwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMjENMAsG
A1UEAwwEU0FSTIIB/zANBgkqhkiG9w0BAQsFAAOCAQEAKnCMDcfZvSdoJgrNdD+1L6p3JQdV73n3
3MRXZ6l5sw4QCdNovbCDAfBERfbAvbrMy2wn+ww/X9RkoBXk+mQQLaZhjOCKeo4npJbpCUdBl8qF
w85mxUZzYQ1S411/R4fYD2h2ejJBTgDyqVeLRLi9LAiihG+goB1WdS2EhUi9N3Sqzv6i+VjT226y
6dKKvxfTTecN5T6Vo4yDUlOCZ2LX2EphF3i554auyl0IGeZ1aC5UXPUXAPePaNKQnbsMeQMoO5Xi
34RYH5uDb4RgsXp3CzpN6CD0nhfmRusQyeLISqd1qO5f9ZOwS0cDifpwojy0WAI0ATWnzGTe/rDI
dUHfzQ==
</xa:EncapsulatedCRLValue></xa:CRLValues></xa:RevocationValues></xa:UnsignedSignatureProperties></xa:UnsignedProperties></xa:QualifyingProperties></xs:Object></xs:Signature></PrescriptionSign>
		</ReferencePrescription>
		<DispensingDocument id=""DispensingDocument"">Q0oxLAoxLOmbu+WHpuOAgOacieWKueS6lOS6jCwyLDIwMTEwMTAxLCwsLCwsLO++g+++nu++ne+9vyDvvpXvvbPvvbrvvbPvvbrvvp7vvoYKMiwxLOaCo+iAheeJueiomOWGheWuue+8kO+8kO+8kSwKNCzmiYvluLPjg6Hjg6Lmg4XloLEwMDEsMjAyMjA3MTMsCjUsMjAyMjA5MDgsCjYsMSwzMTEzMDg0MiwyMDIyLDAxMDAwMSwwMQo3LDE0MzllMGM1LTdjMWQtNDUxMi04NjFkLTI2M2RlNDg0MTdiZiwKMTEs5qSc6Ki855So6Jas5bGALDk5LDQsODAwMDAxNCw0NTYtMDAwMSzplbfltI7nnIzplbfltI7luIIsLAoxNSzolqzliaTluKvmvKLlrZflp5PjgIDmvKLlrZflkI0wMSwsCjUxLOaknOiovOeUqOeXhemZoiw5OSwxLDgwMDAwMTAsCjU1LOWMu+W4q+OAgO+8kO+8kemDjizlhoXnp5EsCjIwMSwxLOODn+ODi+ODquODs+ODoeODq+ODiO+8r++8pOmMoO+8ku+8lc68772HLDIs6YygLDIsNjIyNjg4MTAxLAoyODEsMSzolqzlk4Hoo5zotrPmg4XloLHvvJDvvJEsCjI5MSwxLOiWrOWTgeacjeeUqOazqOaEj+WGheWuuTAxLAozMDEsMSzvvJHml6XvvJHlm57lsLHlr53liY3jgIDmnI3nlKgsMTQs5pel5YiGLDEsMywxMDExMTAwMDAwMDAwMDAwLAozMTEsMSznlKjms5Xoo5zotrPmg4XloLHvvJDvvJEsCjM5MSwxLOWHpuaWueacjeeUqOazqOaEj+aDheWgsTAwMSwKMjAxLDIs44Ki44Or44Oh44K/6Luf6IaP44CA77yQ77yO77yR77yFLDEs5pysLDIsNjYyNjQwNjgzLAoyODEsMizolqzlk4Hoo5zotrPmg4XloLHvvJDvvJEsCjI5MSwyLOiWrOWTgeacjeeUqOazqOaEj+WGheWuuTAxLAozMDEsMizpganlrpzjgIDmnI3nlKgsMSzoqr/liaQsNSwzLDEwNTBXMjAwMDAwMDAwMDAsCjMxMSwyLOeUqOazleijnOi2s+aDheWgse+8kO+8kSwKMzkxLDIs5Yem5pa55pyN55So5rOo5oSP5oOF5aCxMDAxLAo0MDEs5pyN55So5rOo5oSP5oOF5aCxMDAxLAo0MTEs5Lyd6YGU5YaF5a65MDAyXzEsOTksCjUwMSzlgpnogIPvvJDvvJEsCjUxMSw5OTks55aR576p54Wn5Lya57WQ5p6c5YaF5a65MDAx</DispensingDocument>
	</Dispensing>
	<DispensingSign><xs:Signature xmlns:xs=""http://www.w3.org/2000/09/xmldsig#"" Id=""DispensingSign""><xs:SignedInfo Id=""ida2bde79b-SignedInfo""><xs:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/><xs:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256""/><xs:Reference Id=""iddb33cd61-Reference1-Detached"" URI=""#Dispensing""><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>qjvcVHTSZQBaJROD4D0J54ak69eLmC2UyFpz2I6mxdU=
</xs:DigestValue></xs:Reference><xs:Reference Id=""idfbb8185d-Reference2-KeyInfo"" URI=""#id771d57b8-KeyInfo""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>9RQdeILUBLDBtA9WJcJX7stcqrfUaMAsTgmn69c0Wgg=
</xs:DigestValue></xs:Reference><xs:Reference Id=""id5813f70a-Reference3-SignedProperties"" Type=""http://uri.etsi.org/01903#SignedProperties"" URI=""#id111a1e5c-SignedProperties""><xs:Transforms><xs:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/></xs:Transforms><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>sVET1/3caf+ZOcqJ1TPQiM4BxzdzDTUOrbPT526sndw=
</xs:DigestValue></xs:Reference></xs:SignedInfo><xs:SignatureValue Id=""id218934fe-SignatureValue"">adPVPMJsBVQKUA2EHQ6XCqsJr1Hd63LERQSVyb5+shdZSWK8VDtWgRwt5nUOL9Tjz//WVrYXbhsy
bz6V5uscV9yCjRGRvjRblpK+kjO4BpY+yuP0UGCQD2IM61s0GPEdaLbPjS9Iuvik6Y+WnrrvYoCU
VtjzDpWShw4ZbSYkc7s54hXY22UEmM5C953EBLVEtG1mOb+60vGC0gHiiXakFHIIfk8h3a4aJJBB
jW2MCCsNxy07cY1c5EQr9vTKtgCTfUMP2CNpjo7gY2wCySdtE1bbWs60hf+bFWGu6+Rp4ec5gDLX
qRMvyaDt7oYwfS06NishUQz/AlJPHrpGemiMkA==
</xs:SignatureValue><xs:KeyInfo Id=""id771d57b8-KeyInfo""><xs:X509Data><xs:X509Certificate>MIIFeDCCBGCgAwIBAgICAWQwDQYJKoZIhvcNAQELBQAwZjELMAkGA1UEBhMCSlAxDjAMBgNVBAoM
BU1FRElTMRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQDDCZIUEtJLTAxLU1lZGlzU2ln
bkNBMi1mb3JOb25SZXB1ZGlhdGlvbjAeFw0yMjAyMDYxNTAwMDBaFw0yNzAyMDcxNDU5NTlaMGEx
CzAJBgNVBAYTAkpQMSIwIAYDVQQKDBlNRURJUyBVTklWRVJTSVRZIEhPU1BJVEFMMRkwFwYDVQQD
DBBTaGlqdSBLYWd1cmF6YWthMRMwEQYDVQQFEwpUZXN0MzE3MDQ0MIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEAibbTzrF2p/cZD7J8pUhVCBIY+NUjFxIYwT33HmcC9M56GmEdbcdJSuZU
laDbvRtnsmQID3j41yQklo3+FnhVzWlsytGv4l4gWTyI6z7JIl20srJ4wLN/nWeqCOWBbnuSQgbR
ECM8s1rn4tSL5ScLhKdlzNoyjzspIdRjSUfQlsjD1kt0Y2bkdR+g1jEMEthJsBhI13FFgNILVUIe
G3Qfjh0fQCFG57NnJK5pZOBvuAsJq64b0CQnQZ3PPV9y4oOrfdrJsn58mO4lai0B/GKWEoW+8+AB
//CTYgADWpZkyLUtX2+E8/abjyDHeRg6b+OVXO9lRgV1t1NK8n42WSvVOQIDAQABo4ICMzCCAi8w
gcgGA1UdIwSBwDCBvYAUndYTUkXvvV54UshYjni8VB4/luahgaGkgZ4wgZsxCzAJBgNVBAYTAkpQ
MS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFyZTE8MDoGA1UE
CwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFsdWF0aW9uMR0w
GwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMoIBBDAdBgNVHQ4EFgQU4cw98uz7Vevn/sXEneUU
sxLo6EcwDgYDVR0PAQH/BAQDAgZAMG8GA1UdEQRoMGakZDBiMQswCQYDVQQGEwJKUDEhMB8GA1UE
CgwY5Yy755mC5oOF5aCx5aSn5a2m55eF6ZmiMRswGQYDVQQDDBLnpZ7mpb3lnYLjgIDlm5vljYEx
EzARBgNVBAUTClRlc3QzMTcwNDQwOwYDVR0fBDQwMjAwoC6gLIYqaHR0cDovL2NlcnQubWVkaXMu
b3IuanAvc2lnbi9jcmwtc2lnbjIuY3JsMDkGA1UdCQQyMDAwLgYGKIGFQgABMSQxIjAgoB4xHKAM
BgoqgwiGkQ8BBgEBogwMClBoYXJtYWNpc3QwSgYDVR0gAQH/BEAwPjA8BgwqgwiGkQ8BBQEBAAEw
LDAqBggrBgEFBQcCARYeaHR0cDovL3d3dy5tZWRpcy5vci5qcC84X2hwa2kvMA0GCSqGSIb3DQEB
CwUAA4IBAQA4jj9z/P8z3d5+yKBCc/LRBS5SX6omWxYTBjkpvwJ3PyHiKEJcUnir6h214FmOZq8L
Fvjr5I81CEbsmowIhbXHFOd5W4JkD3egx1WGDQaKfoW/GjJA8EIX5CkBYo+FClnOXqPJ+qMmF9IJ
lPmbii0jEuaigkfMytEppfZXOVatPxbFkiDI3aj9jaapb2i26z2iCWPRycLIu4eKZzMRAJ/JXYlB
wpGEHtsWWtT0Gf9lfCVbELpdFyxvQwlbSKdN1b0JAyUsJyYm0TYPCmeddqlbsaxqdEEdyyndqYtX
u2Ct7Jpb73RNkUO1v7nNqAdl4Yj8h8Ppa7hUWTQPozC8CygJ
</xs:X509Certificate></xs:X509Data></xs:KeyInfo><xs:Object xmlns:xa=""http://uri.etsi.org/01903/v1.3.2#""><xa:QualifyingProperties Id=""idca32a483-QualifyingProperties"" Target=""#DispensingSign""><xa:SignedProperties Id=""id111a1e5c-SignedProperties""><xa:SignedSignatureProperties><xa:SigningTime>2022-09-08T09:25:11+00:00</xa:SigningTime><xa:SigningCertificateV2><xa:Cert><xa:CertDigest><xs:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/><xs:DigestValue>XhDf/S8dfQgX4rbkJlcfNjWd9IUhN/y157oKgBPtUzY=
</xs:DigestValue></xa:CertDigest></xa:Cert></xa:SigningCertificateV2></xa:SignedSignatureProperties></xa:SignedProperties></xa:QualifyingProperties></xs:Object></xs:Signature></DispensingSign>
</Document>";

	}
}
