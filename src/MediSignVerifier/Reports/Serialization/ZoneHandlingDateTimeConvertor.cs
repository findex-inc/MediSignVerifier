using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports.Serialization
{
	internal class ZoneHandlingDateTimeConvertor : JsonConverter<DateTime>
	{

		public ZoneHandlingDateTimeConvertor(DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind)
		{
			DateTimeZoneHandling = dateTimeZoneHandling;
		}

		public DateTimeZoneHandling DateTimeZoneHandling { get; }

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			switch (DateTimeZoneHandling) {
				case DateTimeZoneHandling.Local:
					var local = (value.Kind == DateTimeKind.Unspecified)
						? new DateTime(value.Ticks, DateTimeKind.Local)
						: value.ToLocalTime();
					writer.WriteStringValue(local);
					break;

				case DateTimeZoneHandling.Utc:
					var utc = (value.Kind == DateTimeKind.Unspecified)
						? new DateTime(value.Ticks, DateTimeKind.Utc)
						: value.ToUniversalTime();
					writer.WriteStringValue(utc);
					break;

				case DateTimeZoneHandling.RoundtripKind:
					writer.WriteStringValue(value);
					break;

				default:
					throw new NotSupportedException($"{DateTimeZoneHandling} is not supported.");
			}
		}
	}
}
