using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using SignatureVerifier.Reports.Serialization;

namespace SignatureVerifier.Reports.SummaryBuilders
{
	internal class MicrosoftSummarySignatureReportBuilder : SummarySignatureReportBuilder
	{

		public MicrosoftSummarySignatureReportBuilder(string report, SummarySignatureReportConfig config = null)
			: base(report, config)
		{
		}


		public override string Build()
		{
			var option = new JsonDocumentOptions
			{
				AllowTrailingCommas = true,
			};

			var obj = JsonNode.Parse(Original, documentOptions: option);
			IDictionary<string, JsonNode> nodesRef = new Dictionary<string, JsonNode>();

			var tokens = Config.Entries
					.Where(x => !string.IsNullOrEmpty(x))
					.SelectMany(x => obj.TinyQuerySelectorAll(x));

			foreach (var token in tokens) {

				SetEntry(nodesRef, token);
			}

			return nodesRef["$"].ToJsonString(
				new JsonSerializerOptions
				{
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					WriteIndented = IsFormatting
				});
		}


		private static void SetEntry(IDictionary<string, JsonNode> nodesRef, JsonNode entry)
		{
			JsonNode current = entry;
			JsonNode children = null;
			string childrenPath = null;

			while (current != null) {

				var added = false;

				switch (current) {

					case JsonValue _:

						children = current.Copy();
						childrenPath = current.GetPath();
						break;

					case JsonObject _:

						if (children != null) {

							children = SetChildrenToObject(nodesRef, current, children,
								childrenPath);
							childrenPath = current.GetPath();
							added = (children == null);
						}
						else {

							children = current.Copy();
							childrenPath = current.GetPath();
						}
						break;

					case JsonArray _:

						if (children != null) {

							children = SetChildrenToArray(nodesRef, current, children);
							childrenPath = current.GetPath();
							added = (children == null);
						}
						else {

							children = current.Copy();
							childrenPath = current.GetPath();
						}
						break;

					default:
						break;
				}

				if (added) {

					break;
				}

				current = current.Parent;
			}
		}


		private static JsonNode SetChildrenToObject(IDictionary<string, JsonNode> nodesRef, JsonNode current, JsonNode children,
			string childrenPath)
		{
			var path = current.GetPath();
			var name = childrenPath.Split('.').Last();

			if (!nodesRef.TryGetValue(path, out var parent)) {

				var newparent = new JsonObject
				{
					{ name, children }
				};

				nodesRef.Add(path, newparent);

				return newparent;
			}

			parent.AsObject().Add(name, children);

			return null;
		}


		private static JsonNode SetChildrenToArray(IDictionary<string, JsonNode> nodesRef, JsonNode current, JsonNode children)
		{
			var path = current.GetPath();

			if (!nodesRef.TryGetValue(path, out var parent)) {

				var newparent = new JsonArray
				{
					children
				};

				nodesRef.Add(path, newparent);

				return newparent;
			}

			parent.AsArray().Add(children);

			return null;
		}


	}
}
