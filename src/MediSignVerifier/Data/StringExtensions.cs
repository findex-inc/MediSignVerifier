using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SignatureVerifier.Data
{
	internal static class StringExtensions
	{
		public static byte[] ToBytes(this string source)
		{
			return Convert.FromBase64String(source);
		}

		public static string ToBase64String(this byte[] source)
		{
			return Convert.ToBase64String(source);
		}

		public static byte[] ToByteArray(this Stream stream)
		{
			using (MemoryStream ms = new MemoryStream()) {
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static string ToHexString(this byte[] source)
		{
			int i;
			var sOutput = new StringBuilder(source.Length);
			for (i = 0; i < source.Length; i++) {
				sOutput.Append(source[i].ToString("X2"));
			}
			return sOutput.ToString();
		}

		public static string PaddingHex(this string source)
		{
			var result = source;
			if (source.Length % 2 == 0) {

				if (Regex.IsMatch(source, @"^[0-9]+$", RegexOptions.Compiled)) {
					result = $"00{source}";
				}
			}
			else {

				result = $"0{source}";
			}

			return result;
		}

		public static byte[] RemoveUTF8BOM(this byte[] bytes)
		{
			var bom = Encoding.UTF8.GetPreamble();

			// Since the range operator is C# 8.0 or later, it is done in Linq without considering performance.

			if ((bytes.Length > 2) && (bom.SequenceEqual(bytes.Take(bom.Length)))) {

				return bytes.Skip(bom.Length).ToArray();
			}

			return bytes;
		}

	}
}
