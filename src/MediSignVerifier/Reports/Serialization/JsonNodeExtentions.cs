using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SignatureVerifier.Reports.Serialization
{
	internal static class JsonNodeExtentions

	{
		public static JsonNode Copy(this JsonNode source)
			=> source.Deserialize<JsonNode>();


		public static IEnumerable<JsonNode> TinyQuerySelectorAll(this JsonNode source, string selector)
		{
			if (string.IsNullOrEmpty(selector)) { throw new ArgumentNullException(nameof(selector)); }

			if (selector == "$") {

				return new[] { source };
			}

			var selectors = Regex.Split(selector, @"(?<!(@))\.");
			IEnumerable<JsonNode> nodes = new[] { source };

			foreach (var nodeSelector in selectors) {

				var news = nodes.SelectMany(node =>
				{
					return TinySelectNodes(node, nodeSelector);
				});

				nodes = news;
			}

			return nodes;
		}


		private static IEnumerable<JsonNode> TinySelectNodes(this JsonNode node, string nodeSelector)
		{
			if (nodeSelector == "$") {

				yield return node;
				yield break;
			}

			var matchArrayDef = Regex.Match(nodeSelector, @"^([^\[]+)\[([^\]]+)\]", RegexOptions.Compiled);
			if (matchArrayDef.Success) {

				var arrayName = matchArrayDef.Groups[1].Value;
				var indexOrQuery = matchArrayDef.Groups[2].Value;

				var arrayNode = node[arrayName];
				if (arrayNode is null) {

					yield break;
				}

				var array = TinySelectArrayNodes(arrayNode.AsArray(), indexOrQuery);
				foreach (var item in array) {

					yield return item;
				}
			}
			else {

				var item = node[nodeSelector];
				if (item != null) {

					yield return item;
				}
			}
		}


		private static IEnumerable<JsonNode> TinySelectArrayNodes(JsonArray arrayNode, string indexOrQuery)
		{
			if (indexOrQuery == "*") {

				foreach (var item in arrayNode) {

					yield return item;
				}
				yield break;
			}

			if (int.TryParse(indexOrQuery, out var index)) {

				if (index < arrayNode.Count) {

					yield return arrayNode[index];
				}
				yield break;
			}

			var nodes = TinyQueryArrayNodes(arrayNode, indexOrQuery);
			foreach (var item in nodes) {

				yield return item;
			}

		}


		private static IEnumerable<JsonNode> TinyQueryArrayNodes(JsonArray arrayNode, string query)
		{
			// Supported only:
			//		"?(@.name == '21')"
			//		"?(@.name != '21')"

			var matchQuerydef = Regex.Match(query, @"^\?\(@\.(\S+?)\s*(==|!=)\s*'?(\S+?)'?\)$", RegexOptions.Compiled);
			if (!matchQuerydef.Success) {

				throw new InvalidOperationException($"invalid query: {query}");
			}

			var name = matchQuerydef.Groups[1].Value;
			var ope = matchQuerydef.Groups[2].Value;
			var expected = matchQuerydef.Groups[3].Value;

			foreach (var item in arrayNode) {

				var property = item[name];
				if (property != null) {

					var actual = property.ToString();
					if (IsMatch(name, ope, expected, actual)) {

						yield return item;
					}
				}

			}
		}


		private static bool IsMatch(string name, string ope, string expected, string actual)
		{
			switch (ope) {
				case "==":
					return string.Equals(expected, actual);

				case "!=":
					return !string.Equals(expected, actual);

				default:
					throw new InvalidOperationException($"invalid query operator in {name}: {ope}");
			}
		}
	}
}
