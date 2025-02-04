using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using NLog;

namespace SignatureVerifier.Verifiers.StructureVerifiers
{
	/// <summary>
	/// XMLスキーマ検証器
	/// </summary>
	internal class XmlSchemaVerifier : IStructureVerifier
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

#pragma warning disable IDE0052
		private readonly VerificationConfig _config;
#pragma warning restore IDE0052
		private readonly IList<VerificationResultItem> _results;

		public XmlSchemaVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}

		public VerificationResult Verify(ISignedDocument doc)
		{
			_logger.Trace("XMLスキーマ検証を開始します。");

			var document = doc.Raw as XmlDocument;
			VerifySchema(document, doc.DocumentType);

			var status = _results.Select(x => x.Status).ToConclusion();

			_logger.Debug($"XMLスキーマ検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}

		private void VerifySchema(XmlDocument doc, DocumentType _)
		{
			try {
				var schemaSet = new XmlSchemaSet();

				var schemaSig = XmlSchema.Read(new XmlTextReader(new StringReader(MediSignVerifier.Properties.Resources.xmldsig_core_schema)), null);
				var schema132 = XmlSchema.Read(new XmlTextReader(new StringReader(MediSignVerifier.Properties.Resources.XAdES01903v132_201601)), null);
				var schema141 = XmlSchema.Read(new XmlTextReader(new StringReader(MediSignVerifier.Properties.Resources.XAdES01903v141_201601)), null);

				schemaSet.Add(schemaSig);
				schemaSet.Add(schema132);
				schemaSet.Add(schema141);

#if NET462_OR_GREATER
				RemoveUnnecessarySchemas(schemaSet);

#elif NETSTANDARD2_1_OR_GREATER
#else
#endif
				schemaSet.Compile();

				var settings = new XmlReaderSettings
				{
					ValidationType = ValidationType.Schema
				};
				settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
				settings.ValidationEventHandler += ValidationEventHandler;
				settings.Schemas = schemaSet;

				var xmlReader = new XmlTextReader(new StringReader(doc.OuterXml));
				using (var reader = XmlReader.Create(xmlReader, settings)) {
					while (reader.Read()) ;
				}
			}
			catch (Exception ex) {
				_logger.Error(ex);
				DoPostVerification(VerificationStatus.INVALID, SignatureSourceType.None, "XMLスキーマ", "", "不明なエラー");
			}
		}


#if NET462_OR_GREATER

		private void RemoveUnnecessarySchemas(XmlSchemaSet schemaSet)
		{
			// ***********************************************************************
			// *.xsd import element parsing results differ between System.Xml.dll (net462) and netstandard.dll=System.Private.Xml.dll(net6.0).
			// net462 resolves the XAdES132 import schema described in XAdES141, but the import is ignored in net6.0.
			// Even in net6.0, it seems that the import schema can be resolved by setting up a resolver from outside.
			// No documentation regarding specification changes was found.

			// Currently implemented with net6.0 behavior, but it seems difficult to change with external elements.
			// To compile the XmlSchemaSet, you need to remove unnecessary schemas added to net462.
			// ***********************************************************************

			var duplicates = schemaSet.Schemas().Cast<XmlSchema>()
					.GroupBy(x => x.TargetNamespace)
					.Where(x => x.Count() > 1)
					.ToArray();

			foreach (var duplicatee in duplicates.SelectMany(x => x)) {

				schemaSet.Remove(duplicatee);
			}

			foreach (var single in duplicates.Select(x => x.First())) {

				schemaSet.Add(single);
			}

			return;
		}

#elif NETSTANDARD2_1_OR_GREATER
#else
#endif

		private void ValidationEventHandler(object sender, ValidationEventArgs e)
		{

			var message = $"LineNumber={e.Exception?.LineNumber},LinePosition={e.Exception?.LinePosition},{e.Message}";

			switch (e.Severity) {
				case XmlSeverityType.Error:

					_logger.Error(message);
					DoPostVerification(VerificationStatus.INVALID, SignatureSourceType.None, "XMLスキーマ", "", message);
					break;

				case XmlSeverityType.Warning:

					_logger.Warn(message);
					break;
			}
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType sigType, string itemName, string mappedName, string message)
		{
			var item = new VerificationResultItem(status, sigType, AggregateStructureVerifier.Name, itemName, mappedName, message);
			_results.Add(item);

			if (status != VerificationStatus.VALID) { //or == INVALID
				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(status, $"{item.Source}：{item.MappedItem}", item.Message));
			}
		}
	}
}
