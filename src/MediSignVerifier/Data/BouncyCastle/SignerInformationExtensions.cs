using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class SignerInformationExtensions
	{
		public static DerObjectIdentifier GetDigestAlgorithm(this SignerInformation signerInfo)
		{
			return signerInfo.DigestAlgorithmID.Algorithm;
		}

		public static DerObjectIdentifier GetSignatureAlgorithm(this SignerInformation signerInfo)
		{
			return signerInfo.EncryptionAlgorithmID.Algorithm;
		}
	}
}
