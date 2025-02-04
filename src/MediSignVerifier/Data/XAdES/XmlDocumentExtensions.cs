using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class XmlDocumentExtensions
	{
		public static XmlNamespaceManager CreateXAdESNamespaceManager(this XmlDocument doc)
		{
			var nsManager = new XmlNamespaceManager(doc.NameTable);

			nsManager.AddNamespace("xs", "http://www.w3.org/2000/09/xmldsig#");
			nsManager.AddNamespace("xa", "http://uri.etsi.org/01903/v1.3.2#");
			nsManager.AddNamespace("xa141", "http://uri.etsi.org/01903/v1.4.1#");

			return nsManager;
		}

		public static DocumentType GetDocumentType(this XmlDocument doc, XmlNamespaceManager nsManager)
		{
			if (IsPrescriptionDocument(doc, nsManager)) return DocumentType.Prescription;
			if (IsDispensingDocument(doc, nsManager)) return DocumentType.Dispensing;

			return DocumentType.Unknown;
		}

		private static bool IsPrescriptionDocument(XmlDocument doc, XmlNamespaceManager nsManager)
		{
			return doc.SelectSingleNode("/Document/Prescription", nsManager) != null;
		}

		private static bool IsDispensingDocument(XmlDocument doc, XmlNamespaceManager nsManager)
		{
			return doc.SelectSingleNode("/Document/Dispensing", nsManager) != null;
		}
	}
}
