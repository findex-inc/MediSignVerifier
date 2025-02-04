
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	internal static class TransformChainExtensions
	{
		public static Stream GetOutput(this TransformChain chain, XmlElement elem, bool discardComments)
		{
			var baseUri = elem.OwnerDocument.BaseURI;
			var resolver = new XmlSecureResolver(new XmlUrlResolver(), baseUri);

			var namespaces = SecurityCryptoXmlHelper.Utils_GetPropagetedAttributes
				.Invoke(null, new object[] { elem.ParentNode as XmlElement });

			var normDocument = (XmlDocument)SecurityCryptoXmlHelper.Utils_PreProcessElementInput
				.Invoke(null, new object[] { elem, resolver, baseUri });

			SecurityCryptoXmlHelper.Utils_AddNameSpaces.Invoke(null, new object[] { normDocument.DocumentElement, namespaces });

			if (discardComments) {
				var docWithNoComments = (XmlDocument)SecurityCryptoXmlHelper.Utils_DiscardComments
					.Invoke(null, new object[] { normDocument });
				return (Stream)SecurityCryptoXmlHelper.TransformChain_TransformToOctetStream
					.Invoke(chain, new object[] { docWithNoComments, resolver, baseUri });
			}
			else {
				return (Stream)SecurityCryptoXmlHelper.TransformChain_TransformToOctetStream
					.Invoke(chain, new object[] { normDocument, resolver, baseUri });
			}
		}

		public static Stream GetOutput(this TransformChain chain, XmlDocument doc)
		{
			var baseUri = doc.BaseURI;
			var resolver = new XmlSecureResolver(new XmlUrlResolver(), baseUri);

			var normDocument = (XmlDocument)SecurityCryptoXmlHelper.Utils_PreProcessDocumentInput
				.Invoke(null, new object[] { doc, resolver, baseUri });

			var docWithNoComments = (XmlDocument)SecurityCryptoXmlHelper.Utils_DiscardComments
				.Invoke(null, new object[] { normDocument });

			return (Stream)SecurityCryptoXmlHelper.TransformChain_TransformToOctetStream
				.Invoke(chain, new object[] { docWithNoComments, resolver, baseUri });
		}

		public static Stream GetOutput(this TransformChain chain, XmlDocument doc, string uri)
		{
			//out 引数の取得が必要
			object[] parameters = new object[] { uri, null };
			var idref = (string)SecurityCryptoXmlHelper.Utils_GetIdFromLocalUri.Invoke(null, parameters);
			var discardComments = (bool)parameters[1];

			var elem = GetXmlElement(doc, idref);

			return chain.GetOutput(elem, discardComments);
		}

		public static void LoadXml(this TransformChain chain, XmlElement elem)
		{
			SecurityCryptoXmlHelper.TransformChain_LoadXml.Invoke(chain, new object[] { elem });
		}

		public static void AddNamespaces(this TransformChain chain, IEnumerable<XmlAttribute> namespaces)
		{
			foreach (var transform in chain) {
				if (transform.GetType() == typeof(XmlDsigXPathTransform)) {
					//namespaceの追加が必要
					var nsm = (XmlNamespaceManager)SecurityCryptoXmlHelper.XmlDsigXPathTransform_NamespaceManager.GetValue(transform);
					foreach (var ns in namespaces) {
						if (nsm.LookupNamespace(ns.LocalName) != null) continue; //HasNamespaceは使えなかった
						nsm.AddNamespace(ns.LocalName, ns.Value);
					}
				}
			}
		}

		private static XmlElement GetXmlElement(XmlDocument doc, string id)
		{
			if (id.StartsWith("#")) id = id.Substring(1);

			var idAttrList = new string[] { "Id", "id", "ID" };
			foreach (var idAttr in idAttrList) {
				string xPath = "//*[@" + idAttr + "=\"" + id + "\"]";
				var nodeList = doc.SelectNodes(xPath);
				if (nodeList.Count > 0) {
					return (XmlElement)nodeList[0]; //TODO
				}
			}

			return null;
		}
	}
}
