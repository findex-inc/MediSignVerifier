using Moq;
using NUnit.Framework;
using MediSignVerifier.Tests.Properties;

namespace SignatureVerifier
{
	internal class SummarySignatureReportBuilderTests
	{

		[Test]
		public void WhenBuildSimpleJson()
		{
			var entries = new[]
			{
				"$.object-array[?(@.name != 3)]",
				"$.int-array",
				"$.object1.name",
			};

			var config = new Mock<SummarySignatureReportConfig>();
			config.SetupGet(x => x.Entries).Returns(entries);

			var input = InputForSimpleJson;
			var actual = SummarySignatureReportBuilder.Create(input, config.Object)
				.Build();

			TestContext.WriteLine(actual);

			var expected = ExpectedForSimpleJson;
			Assert.That(actual, Is.EqualTo(expected));

			return;
		}


		private const string InputForSimpleJson = @"{
 ""int"": 100,
 ""boolean"": true,
 ""string"": ""string value"",
 ""date1"": ""2023-03-20T12.34.56.789+9:00"",
 ""date2"": ""2023-03-20T12.34.56.789Z"",
 ""object1"": { ""name"": 4 },
 ""int-array"": [ 1, 2, 3],
 ""string-array"": [ ""1"", ""2"", ""3""],
 ""object-array"": [ { ""name"": 1}, { ""name"": 2} , {""name"": 3 }],
}";

		private const string ExpectedForSimpleJson
			= "{\"object-array\":[{\"name\":1},{\"name\":2}],\"int-array\":[1,2,3],\"object1\":{\"name\":4}}";


		[Test]
		public void WhenBuild_WithArrayIndex()
		{
			var entries = new[]
			{
				"$.object-array2[2].nested[1].name",
				"$.object-array2[2].value",
				"$.object-array2[2].name",
				"$.object-array1[2].name",
				"$.object-array2[1].name",
				"$.object-array2[2].nested[?(@.name == '12')]",
				"$.object-array1[?(@.name != '1')]",
			};

			var config = new Mock<SummarySignatureReportConfig>();
			config.SetupGet(x => x.Entries).Returns(entries);

			var input = InputForArrayIndexJson;
			var actual = SummarySignatureReportBuilder.Create(input, config.Object)
				.Format(true)
				.Build();

			TestContext.WriteLine(actual);

			var expected = ExpectedForArrayIndexJson;
			Assert.That(actual, Is.EqualTo(expected));

			return;
		}


		private const string InputForArrayIndexJson = @"{
 ""string-array"": [ ""1"", ""2"", ""3""],
 ""object-array1"": [ { ""name"": 1}, { ""name"": 2} , {""name"": 3 }],
 ""object-array2"": [
	{ ""name"": 1, ""value"": ""data1"", ""nested"": [ { ""name"": 11}, { ""name"": 12} , {""name"": 13 }]},
	{ ""name"": 2, ""value"": ""data2"", ""nested"": [ { ""name"": 21}, { ""name"": 22} , {""name"": 23 }]},
	{ ""name"": 3, ""value"": ""data3"", ""nested"": [ { ""name"": 31}, { ""name"": 32} , {""name"": 33 }]}],
}";

		private const string ExpectedForArrayIndexJson
			=
@"{
  ""object-array2"": [
    {
      ""nested"": [
        {
          ""name"": 32
        }
      ],
      ""value"": ""data3"",
      ""name"": 3
    },
    {
      ""name"": 2
    }
  ],
  ""object-array1"": [
    {
      ""name"": 3
    },
    {
      ""name"": 2
    },
    {
      ""name"": 3
    }
  ]
}";


		[Test]
		[Ignore("System.Text.Json not supported")]
		public void WhenBuild_WithStatusInvalidOnly()
		{
			var entries = new[]
			{
				"$..検証結果詳細[?(@.結果 != 'VALID')]"
			};

			var config = new Mock<SummarySignatureReportConfig>();
			config.SetupGet(x => x.Entries).Returns(entries);

			var input = InputForStatusInvalidOnly;
			var actual = SummarySignatureReportBuilder.Create(input, config.Object)
				.Format(true)
				.Build();

			TestContext.WriteLine(actual);

			var expected = ExpectedForStatusInvalidOnly;
			Assert.That(actual, Is.EqualTo(expected));

			return;
		}

		private const string InputForStatusInvalidOnly = @"{
  ""検証ファイル名"": ""XAdES-A.xml"",
  ""レポート作成時刻"": ""2023-03-22T10:06:59.578552+09:00"",
  ""検証時刻"": ""2023-03-22T10:06:59.0021579+09:00"",
  ""検証結果"": ""INVALID"",
  ""電子処方箋種別"": ""Unknown"",
  ""署名リスト"": [
    {
      ""署名No"": 1,
      ""署名種別"": ""Unknown"",
      ""署名フォーマット"": ""A"",
      ""パス"": ""/Signature"",
      ""署名構造"": {
        ""検証結果"": ""INVALID"",
        ""検証結果詳細"": [
          {
            ""結果"": ""VALID"",
            ""検証内容"": ""TEST-1""
          },
          {
            ""結果"": ""INDETERMINATE"",
            ""検証内容"": ""TEST-2""
          },
          {
            ""結果"": ""INVALID"",
            ""検証内容"": ""TEST-3""
          }
        ]
      },
      ""署名データ"": {
        ""検証結果"": ""VALID"",
        ""Id"": ""Id-TEST-4"",
        ""正規化アルゴリズム"": ""C14N"",
        ""署名アルゴリズム"": ""SHA-256withRSA"",
        ""検証結果詳細"": [
          {
            ""結果"": ""VALID"",
            ""検証内容"": ""SignatureValue要素""
          },
          {
            ""結果"": ""VALID"",
            ""検証内容"": ""SignatureMethod要素""
          }
        ]
      },
    }
  ]
}";

		private const string ExpectedForStatusInvalidOnly = @"{
  ""署名リスト"": [
    {
      ""署名構造"": {
        ""検証結果詳細"": [
          {
            ""結果"": ""INDETERMINATE"",
            ""検証内容"": ""TEST-2""
          },
          {
            ""結果"": ""INVALID"",
            ""検証内容"": ""TEST-3""
          }
        ]
      }
    }
  ]
}";


		[Test]
		public void WhenBuild_WithDefaultConfiguration()
		{
			var input = Resources.SummaryReportForDefault;
			var actual = SummarySignatureReportBuilder.Create(input)
				.Format(true)
				.Build();

			TestContext.WriteLine(actual);

			var expected = Resources.SummaryReportForDefaultExpected.TrimEnd();
			Assert.That(actual, Is.EqualTo(expected));

			return;
		}

	}
}
