using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Cms;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal static class CmsSignedDataExtensions
	{
		public static bool IsSignedData(this CmsSignedData data)
		{
			return data.ContentInfo.ContentType.Equals(PkcsObjectIdentifiers.SignedData);
		}

		public static string GetContentType(this CmsSignedData data)
		{
			return data.ContentInfo.ContentType.Id;
		}

		public static bool IsTstInfo(this CmsSignedData data)
		{
			return data.SignedContentType.Equals(PkcsObjectIdentifiers.IdCTTstInfo);
		}

		public static string GetSignedContentType(this CmsSignedData data)
		{
			return data.SignedContentType.Id;
		}
	}
}
