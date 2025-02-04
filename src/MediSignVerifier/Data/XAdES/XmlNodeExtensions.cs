using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class XmlNodeExtensions
	{

		public static bool IsReference(this XmlNode node, string id)
		{
			if (string.IsNullOrEmpty(id)) return false;

			var refId = id.StartsWith("#") ? id : $"#{id}";
			var refUri = node.GetUri();
			var result = refUri == refId;

			return result;
		}

		public static string GetUri(this XmlNode node)
		{
			return node.Attributes.OfType<XmlAttribute>().FirstOrDefault(x => x.Name == @"URI")?.Value;
		}

		public static string GetAncestorId(this XmlNode node)
		{
			var parent = node.ParentNode;
			while (parent != null) {

				var parentId = parent.GetId();
				if (!string.IsNullOrEmpty(parentId)) {

					return parentId;
				}
				parent = parent.ParentNode;
			}

			return null;
		}

		public static string GetId(this XmlNode node)
		{
			return ((XmlElement)node).GetId();
		}

		public static string GetAlgorithm(this XmlNode node)
		{
			return ((XmlElement)node).GetAlgorithm();
		}

		public static string GetNodeName(this XmlNode node, bool additionalId = false)
		{
			return (additionalId)
				? GetNodeNameWithId(node)
				: node.Name;

			string GetNodeNameWithId(XmlNode n)
			{
				var id = n.GetId();
				return string.IsNullOrEmpty(id)
					? n.Name
					: $"{n.Name}[@Id=\"{id}\"]";
			}
		}

		public static string GetAbsolutePath(this XmlNode node, bool additionalId = false)
		{
			var nodes = GetParentNode(node).Select(x => x.Name).Reverse().ToList();
			nodes.Add(node.GetNodeName(additionalId));

			nodes.Insert(0, "");
			var xpath = string.Join("/", nodes);

			return xpath;

			IEnumerable<XmlNode> GetParentNode(XmlNode current)
			{
				var parent = current?.ParentNode as XmlElement;
				while (parent != null) {
					yield return parent;
					parent = parent.ParentNode as XmlElement;
				}
			}

		}
	}
}
