using System.Collections.Generic;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class XmlElementExtensions
	{
		public static string GetId(this XmlElement elem)
		{
			if (elem.HasAttribute("Id")) return elem.GetAttribute("Id");
			if (elem.HasAttribute("id")) return elem.GetAttribute("id");
			if (elem.HasAttribute("ID")) return elem.GetAttribute("ID");

			return null;
		}

		public static string GetUri(this XmlElement elem)
		{
			return elem.GetAttribute("URI");
		}

		public static string GetAlgorithm(this XmlElement elem)
		{
			return elem.GetAttribute("Algorithm");
		}

		public static IEnumerable<XmlAttribute> GetAllNamespaces(this XmlElement elem)
		{
			List<XmlAttribute> namespaces = new List<XmlAttribute>();

			if (elem?.ParentNode?.NodeType == XmlNodeType.Document) {
				foreach (XmlAttribute attr in elem.Attributes) {
					AddNamespace(namespaces, attr);
				}

				return namespaces;
			}

			XmlNode currentNode = elem;

			while (currentNode != null && currentNode.NodeType != XmlNodeType.Document) {
				foreach (XmlAttribute attr in currentNode.Attributes) {
					AddNamespace(namespaces, attr);
				}

				currentNode = currentNode.ParentNode;
			}

			return namespaces;
		}

		private static void AddNamespace(List<XmlAttribute> namespaces, XmlAttribute attr)
		{
			if (attr.Name.StartsWith("xmlns") && !namespaces.Exists(f => f.Name == attr.Name)) {
				namespaces.Add(attr);
			}
		}

	}
}
