using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class MsHashAlgorithmExtensions
	{
		public static DerObjectIdentifier ToDerObjectIdentifier(this HashAlgorithm hashAlgorithm)
		{
			switch (hashAlgorithm) {
				case System.Security.Cryptography.SHA1 _:
					return OiwObjectIdentifiers.IdSha1;

				case System.Security.Cryptography.SHA256 _:
					return NistObjectIdentifiers.IdSha256;

				case System.Security.Cryptography.SHA384 _:
					return NistObjectIdentifiers.IdSha384;

				case System.Security.Cryptography.SHA512 _:
					return NistObjectIdentifiers.IdSha512;

				default:
					throw new NotSupportedException($"Not supported hashAlgorithm {hashAlgorithm}");
			}
		}

		public static DerObjectIdentifier ToDerObjectIdentifier(this string algorithm)
		{
			var hashAlgorithm = SecurityCryptoXmlHelper.CreateHashAlgorithmFromName(algorithm);
			if (hashAlgorithm == null) return null;

			return ToDerObjectIdentifier(hashAlgorithm);
		}
	}
}
