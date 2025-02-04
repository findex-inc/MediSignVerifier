using System.Xml;

namespace SignatureVerifier.Data.XAdES
{
	internal static class TimeStampXmlNodeExtensions
	{
		public static XmlNode GetEncapsulatedTimeStampNode(this XmlNode parent, XmlNamespaceManager nsManager)
		{
			return parent.SelectSingleNode("xa:EncapsulatedTimeStamp", nsManager);
		}
	}
}
