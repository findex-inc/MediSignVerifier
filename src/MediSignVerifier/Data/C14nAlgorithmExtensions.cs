namespace SignatureVerifier.Data
{
	internal static class C14nAlgorithmExtensions
	{
		public static string C14nAlgorithmName(this string uri)
		{
			switch (uri) {
				case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315":
					return "C14N";

				case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments":
					return "C14N(with comments)";

				case "http://www.w3.org/2001/10/xml-exc-c14n#":
					return "EXC-C14N";

				case "http://www.w3.org/2001/10/xml-exc-c14n#WithComments":
					return "EXC-C14N(with comments)";

				default:
					return uri;
			}
		}
	}
}
