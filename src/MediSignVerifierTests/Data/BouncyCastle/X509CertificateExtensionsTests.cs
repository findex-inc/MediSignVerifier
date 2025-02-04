using System;
using System.Linq;
using NUnit.Framework;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.Data.BouncyCastle
{
	public class X509CertificateExtensionsTests
	{
		private readonly string base64 = @"MIIFgjCCBGqgAwIBAgICAV4wDQYJKoZIhvcNAQELBQAwZjELMAkGA1UEBhMCSlAxDjAMBgNVBAoM BU1FRElTMRYwFAYDVQQLDA1NRURJUyBIUEtJIENBMS8wLQYDVQQDDCZIUEtJLTAxLU1lZGlzU2ln bkNBMi1mb3JOb25SZXB1ZGlhdGlvbjAeFw0yMjAyMDYxNTAwMDBaFw0yNzAyMDcxNDU5NTlaMGQx CzAJBgNVBAYTAkpQMSIwIAYDVQQKDBlNRURJUyBVTklWRVJTSVRZIEhPU1BJVEFMMRwwGgYDVQQD DBNTYW5qdXNoaSBLYWd1cmF6YWthMRMwEQYDVQQFEwpUZXN0MTE3MTIwMIIBIjANBgkqhkiG9w0B AQEFAAOCAQ8AMIIBCgKCAQEAy6PVZxPcrXC3jW+teT7YLvl2+0lfHRbZWjf56eMn8mRUKKNsuPAu tvSaTbRBL+E4S5HIzYR7dUtqqXnRg+xH1YPe2eaFAfzJFBNZggHTjYfV2wmX0kE/0sssjy5A1BUZ 4zRN1+oGantT8CkLJD4EsTZX1LCNJwUsBpkzN6kyhc4HddG9M3ISGa0dMtP31L1XcjCs/cP3xCJb 8Z5osEacccwVBaiL7CdpATEcro+xEJtYii0IibH9KJcrxOEbUkP8KWTzC2npwA+YauAmC6s8GfMr wAEb0U8x+v58gUe4hVJS6PmW3QhnwVHFKy5Yp6zbDxB2jEomOp8iIvsSUXBBMwIDAQABo4ICOjCC AjYwgcgGA1UdIwSBwDCBvYAUndYTUkXvvV54UshYjni8VB4/luahgaGkgZ4wgZsxCzAJBgNVBAYT AkpQMS8wLQYDVQQKDCZNaW5pc3RyeSBvZiBIZWFsdGgsIExhYm91ciBhbmQgV2VsZmFyZTE8MDoG A1UECwwzRGlyZWN0b3ItR2VuZXJhbCBmb3IgUG9saWN5IFBsYW5uaW5nIGFuZCBFdmFsdWF0aW9u MR0wGwYDVQQLDBRNSExXIEhQS0kgUm9vdCBDQSBWMoIBBDAdBgNVHQ4EFgQUwUIYMzPxAiX6Mwch BNWKrcAL4UUwDgYDVR0PAQH/BAQDAgZAMHIGA1UdEQRrMGmkZzBlMQswCQYDVQQGEwJKUDEhMB8G A1UECgwY5Yy755mC5oOF5aCx5aSn5a2m55eF6ZmiMR4wHAYDVQQDDBXnpZ7mpb3lnYLjgIDkuInl jYHlm5sxEzARBgNVBAUTClRlc3QxMTcxMjAwOwYDVR0fBDQwMjAwoC6gLIYqaHR0cDovL2NlcnQu bWVkaXMub3IuanAvc2lnbi9jcmwtc2lnbjIuY3JsMD0GA1UdCQQ2MDQwMgYGKIGFQgABMSgxJjAk oCIxIKAMBgoqgwiGkQ8BBgEBohAMDk1lZGljYWwgRG9jdG9yMEoGA1UdIAEB/wRAMD4wPAYMKoMI hpEPAQUBAQABMCwwKgYIKwYBBQUHAgEWHmh0dHA6Ly93d3cubWVkaXMub3IuanAvOF9ocGtpLzAN BgkqhkiG9w0BAQsFAAOCAQEAmUx6UQDm6HboCK39k2/6qL9uAB9kSQtlyLxQ8Jpwdb7Q8iqNfyyo ZXfMArjQpEcCQc63fsNoDItQfFjI8YJPMZExAVsPI1n/3NnelX00rxshPvZQvsryYntNUMykBbro hlKx1hQJFCIETzXeBKrNJIHTvy+WF/3nAsIp6hU3mu2y9qzPhX0UViIlJQbGEhIj5JXmNYaVpYwR DqOArUheAmbDevNNXKRsEfNICmtxotY1WlPdbneh3axHutxrzs5E4i+0XCr8m+C2Fz+bQoChiMTP 5R4USWe3R3Nt+dNZjbj1OvEnLEUT5tOfpZ0KzXDJZht5DOHLUhtV6+WuI+6Yqw== ";

		[Test]
		public void WhenCallingGetHcRoleWithBouncyCastle()
		{
			byte[] der = Convert.FromBase64String(base64);
			var certificate = new X509CertificateParser().ReadCertificate(der);
			TestContext.WriteLine(certificate.DumpSubjectDirectoryAttributesAsString());

			var hcRoles = certificate.GetHcRoles();

			Assert.That(hcRoles.Count(), Is.EqualTo(1));

			var hcRole = hcRoles.First();
			Assert.Multiple(() =>
			{
				Assert.That(hcRole.Oid, Is.EqualTo("1.2.392.100495.1.6.1.1"));
				Assert.That(hcRole.RoleName, Is.EqualTo("Medical Doctor"));
			});
			return;
		}
	}
}
