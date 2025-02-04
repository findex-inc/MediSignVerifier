using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Security;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class DerObjectIdentifierExtensions
	{
		public static string DigestAlgorithmName(this DerObjectIdentifier algorithm)
		{
			//定義がなければoidを返す
			return DigestUtilities.GetAlgorithmName(algorithm) ?? algorithm.Id;
		}

		public static string SignatureAlgorithmName(this DerObjectIdentifier algorithm)
		{
			if (algorithm.Id == PkcsObjectIdentifiers.RsaEncryption.Id) {
				//Sha1WithRsaEncryption だったら変換できるんだけどなあ・・。
				return "RSA";
			}
			else {
				//定義がなければoidを返す
				return SignerUtilities.GetEncodingName(algorithm) ?? algorithm.Id;
			}
		}

		public static bool IsSupportedDigestAlgorithm(this DerObjectIdentifier algorithm)
		{
			//名称=oidの場合は非対応とみなす
			return algorithm.DigestAlgorithmName() != algorithm.Id;
		}

		public static bool IsSupportedSignatureAlgorithm(this DerObjectIdentifier algorithm)
		{
			//名称=oidの場合は非対応とみなす
			return algorithm.SignatureAlgorithmName() != algorithm.Id;
		}

	}
}
