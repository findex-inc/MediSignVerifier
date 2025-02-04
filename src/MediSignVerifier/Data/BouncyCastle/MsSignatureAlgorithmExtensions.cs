using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class MsSignatureAlgorithmExtensions
	{
		/// <summary>
		/// SignatureAlgorithmURIをBouncyCastleMechanismに変換
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		internal static DerObjectIdentifier ToBCMechanism(this string uri)
		{
			string algorithm = null;
			switch (uri) {
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha224":
					algorithm = "SHA-224withRSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
					algorithm = "SHA-256withRSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384":
					algorithm = "SHA-384withRSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
					algorithm = "SHA-512withRSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha224":
					algorithm = "SHA-224withECDSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256":
					algorithm = "SHA-256withECDSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha384":
					algorithm = "SHA-384withECDSA";
					break;
				case "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha512":
					algorithm = "SHA-512withECDSA";
					break;
			}

			if (string.IsNullOrEmpty(algorithm)) return null;

			return SignerUtilities.GetObjectIdentifier(algorithm);
		}
	}
}
