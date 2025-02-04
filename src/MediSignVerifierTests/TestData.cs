using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using SignatureVerifier.Data;
using SignatureVerifier.Reports.Serialization;

namespace SignatureVerifier
{
	internal static class TestData
	{
		internal static void SetESLevel(ESLevel level, ISignature signature)
		{
			if (signature is SignatureXml xml) {
				xml.ESLevel = level;
			}
		}

		internal static XmlDocument CreateXmlDocument(string xmlString)
		{
			var doc = new XmlDocument
			{
				PreserveWhitespace = true,
			};

			doc.LoadXml(xmlString);
			return doc;
		}

		internal static string ToJson<T>(T obj, DateTimeZoneHandling timezone = DateTimeZoneHandling.Local)
		{
			var settings = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = true,
			};
			settings.Converters.Add(new JsonStringEnumConverter());
			settings.Converters.Add(new ZoneHandlingDateTimeConvertor(dateTimeZoneHandling: timezone));

			return JsonSerializer.Serialize<T>(obj, settings);
		}
	}

}
