using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SignatureVerifier.Data;
using SignatureVerifier.Data.XAdES;
using SignatureVerifier.Verifiers.StructureVerifiers;

namespace SignatureVerifier
{
	/// <summary>
	/// 電子署名XML
	/// </summary>
	public class SignedDocumentXml : ISignedDocument
	{
		private readonly string _filePath;
		private readonly XmlDocument _doc;
		private readonly XmlNamespaceManager _nsManager;
		private readonly DocumentType _documentType;
		private readonly IList<ISignature> _signatures;

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="document">電子処方箋XML</param>
		/// <param name="filePath">ファイルパス</param>
		public SignedDocumentXml(XmlDocument document, string filePath)
		{
			_filePath = filePath;
			_doc = document;
			_nsManager = _doc.CreateXAdESNamespaceManager();
			_documentType = _doc.GetDocumentType(_nsManager);

			_signatures = new List<ISignature>();
			CreateSignatureXml();
		}

		private void CreateSignatureXml()
		{
			//XML内のSignature要素を列挙
			foreach (var sigNode in _doc.SelectNodes("//xs:Signature", _nsManager).OfType<XmlNode>()) {
				_signatures.Add(new SignatureXml(sigNode, _doc, _nsManager));
			}
		}

		/// <inheritdoc/>
		public DocumentType DocumentType => _documentType;

		/// <inheritdoc/>
		public string FilePath => _filePath;

		/// <inheritdoc/>
		public object Raw => _doc;

		/// <inheritdoc/>
		public IEnumerable<ISignature> Signatures => _signatures;

		/// <summary>
		/// 電子処方箋XMLをロード
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <returns>ロードした電子署名XMLを返却します。</returns>
		public static SignedDocumentXml Load(string path)
		{
			var xmlDocument = new XmlDocument
			{
				PreserveWhitespace = true
			};
			xmlDocument.Load(path);

			return new SignedDocumentXml(xmlDocument, System.IO.Path.GetFullPath(path));
		}

		/// <inheritdoc/>
		public IStructureVerifier CreateStructureVerifier(VerificationConfig config)
		{
			return new AggregateStructureVerifier(config, new IStructureVerifier[]
			{
				new XmlSchemaVerifier(config),
				new XAdESStructureVerifier(config),
			});
		}

	}
}
