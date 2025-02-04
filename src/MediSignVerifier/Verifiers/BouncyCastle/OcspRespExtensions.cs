using System;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Reports;

namespace SignatureVerifier.Verifiers.BouncyCastle
{
	internal static class OcspRespExtensions
	{
		public static void Validate(this OcspResp ocspResp,
			X509Certificate cert,
			DateTime validateTime,
			X509Certificate sign)
		{
			if (!ocspResp.IsMatch(cert, sign)) {

				throw new InvalidOperationException("Specified OCSP and Certificate do not match.");
			}

			var basic = ocspResp.GetResponseObject() as BasicOcspResp;
			foreach (var response in basic.Responses) {
				var status = response.GetCertStatus();
				if (status != CertificateStatus.Good) {
					if (status is RevokedStatus revoked) {

						var formattedDate = revoked.RevocationTime.ToString("ddd MMM dd HH:mm:ss K yyyy");
						var reason = CertificateRevocationReason.Parse(revoked.RevocationReason).Name;
						var message = $"Certificate revocation after {formattedDate}, reason: {reason}";
						throw new ApplicationException(message);
					}

					throw new ApplicationException("Certificate status could not be determined.");
				}

				if (response.ThisUpdate <= validateTime) {
					if ((response.NextUpdate != null) && (validateTime > response.NextUpdate.Value)) {

						throw new ApplicationException("No valid OCSP for current time found.");
					}
				}

			}

			return;
		}

	}
}
