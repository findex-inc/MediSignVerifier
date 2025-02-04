using System;
using NUnit.Framework;

#if USE_NEWTONSOFT
using Newtonsoft.Json;

#else
using System.Text.Json;

#endif

namespace SignatureVerifier.Reports.Serialization
{
	internal class ZoneHandlingDateTimeConvertorTests
	{

		public static readonly object[] LocalTimeData =
		{
			new object []{
				new { Value = DateTime.Parse("2000-02-29") },
				"{\"Value\":\"2000-02-29T00:00:00+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45") },
				"{\"Value\":\"2000-02-29T01:23:45+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567") },
				"{\"Value\":\"2000-02-29T01:23:45.1234567+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.12345678") },
				"{\"Value\":\"2000-02-29T01:23:45.1234568+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567+09:00") },
				"{\"Value\":\"2000-02-29T01:23:45.1234567+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567Z").ToUniversalTime() },
				"{\"Value\":\"2000-02-29T10:23:45.1234567+09:00\"}" },
		};

		[Test(Description = "DateTimeをLocalTimeに変換してシリアライズする。")]
		[TestCaseSource(nameof(LocalTimeData))]
		public void WhenSelializeDatetime_WithLocalTime(object data, string ecxpected)
		{

#if USE_NEWTONSOFT
			var settings = new JsonSerializerSettings
			{
				DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
			};

			var actual = JsonConvert.SerializeObject(data, settings);

#else
			var settings = new JsonSerializerOptions();
			settings.Converters.Add(new ZoneHandlingDateTimeConvertor(DateTimeZoneHandling.Local));

			var actual = JsonSerializer.Serialize(data, data.GetType(), settings);

#endif

			Assert.That(actual, Is.EqualTo(ecxpected));
		}


		public static readonly object[] UtcTimeData =
		{
			new object []{
				new { Value = DateTime.Parse("2000-02-29") },
				"{\"Value\":\"2000-02-29T00:00:00Z\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45") },
				"{\"Value\":\"2000-02-29T01:23:45Z\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567") },
				"{\"Value\":\"2000-02-29T01:23:45.1234567Z\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.12345678") },
				"{\"Value\":\"2000-02-29T01:23:45.1234568Z\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567+09:00") },
				"{\"Value\":\"2000-02-28T16:23:45.1234567Z\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567Z").ToUniversalTime() },
				"{\"Value\":\"2000-02-29T01:23:45.1234567Z\"}" },
		};

		[Test(Description = "DateTimeをUTCに変換してシリアライズする。")]
		[TestCaseSource(nameof(UtcTimeData))]
		public void WhenSelializeDatetime_WithUtcTime(object data, string ecxpected)
		{

#if USE_NEWTONSOFT
			var settings = new JsonSerializerSettings
			{
				DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
			};

			var actual = JsonConvert.SerializeObject(data, settings);

#else
			var settings = new JsonSerializerOptions();
			settings.Converters.Add(new ZoneHandlingDateTimeConvertor(DateTimeZoneHandling.Utc));

			var actual = JsonSerializer.Serialize(data, data.GetType(), settings);

#endif

			Assert.That(actual, Is.EqualTo(ecxpected));
		}


		public static readonly object[] RoundtripKindTimeData =
		{
			new object []{
				new { Value = DateTime.Parse("2000-02-29") },
				"{\"Value\":\"2000-02-29T00:00:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45") },
				"{\"Value\":\"2000-02-29T01:23:45\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567") },
				"{\"Value\":\"2000-02-29T01:23:45.1234567\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.12345678") },
				"{\"Value\":\"2000-02-29T01:23:45.1234568\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567+09:00") },
				"{\"Value\":\"2000-02-29T01:23:45.1234567+09:00\"}" },
			new object []{
				new { Value = DateTime.Parse("2000-02-29T01:23:45.1234567Z").ToUniversalTime() },
				"{\"Value\":\"2000-02-29T01:23:45.1234567Z\"}" },
		};

		[Test(Description = "DateTimeをデータのTimeZoneに合わせてシリアライズする。")]
		[TestCaseSource(nameof(RoundtripKindTimeData))]
		public void WhenSelializeDatetime_WithRoundtripKindTime(object data, string ecxpected)
		{

#if USE_NEWTONSOFT
			var settings = new JsonSerializerSettings
			{
				DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind,
			};

			var actual = JsonConvert.SerializeObject(data, settings);

#else
			var settings = new JsonSerializerOptions();
			settings.Converters.Add(new ZoneHandlingDateTimeConvertor(DateTimeZoneHandling.RoundtripKind));

			var actual = JsonSerializer.Serialize(data, data.GetType(), settings);

#endif

			Assert.That(actual, Is.EqualTo(ecxpected));
		}

	}
}
