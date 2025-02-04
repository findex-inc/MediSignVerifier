using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Data.XAdES;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXml : ISignature
	{
		private readonly XmlDocument _doc;
		private readonly XmlNamespaceManager _nsManager;
		private readonly XmlNode _sigNode;
		private readonly string _path;

		private readonly object _locked = new object();

		public SignatureXml(XmlNode signatureNode, XmlDocument document, XmlNamespaceManager nsManager)
		{
			_sigNode = signatureNode;
			_path = signatureNode?.GetAbsolutePath();
			_doc = document;
			_nsManager = nsManager;
		}

		public ESLevel ESLevel
		{
			get
			{
				// https://codeql.github.com/codeql-query-help/csharp/cs-unsafe-double-checked-lock/
				lock (_locked) {
					_esLevel = _esLevel ?? _sigNode.GetESLevel(_nsManager);
				}
				return _esLevel.Value;
			}
			internal set
			{
				lock (_locked) { _esLevel = value; }
			}
		}
		private ESLevel? _esLevel;

		public SignatureSourceType SourceType
		{
			get
			{
				// https://codeql.github.com/codeql-query-help/csharp/cs-unsafe-double-checked-lock/
				lock (_locked) {
					_sourceType = _sourceType ?? _sigNode.GetSignatureSourceType();
				}
				return _sourceType.Value;
			}
			internal set
			{
				lock (_locked) { _sourceType = value; }
			}
		}
		private SignatureSourceType? _sourceType;

		public string Path => _path;


		public TimeStampValidationData SignatureTimeStampValidationData => GetSignatureTimeStampData();

		public IEnumerable<ReferenceValidationData> ReferenceValidationData => GetReferenceData();

		public SignatureValueValidationData SignatureValueValidationData => GetSignatureValue();

		public DateTime? SignatureTimeStampGenTime
		{
			get
			{
				if (_signatureTimeStampGenTime == null) {
					var timeStampNode = _sigNode.GetSignatureTimeStampNode(_nsManager)?.GetEncapsulatedTimeStampNode(_nsManager);
					_signatureTimeStampGenTime = GetTimeStampGenTime(timeStampNode);
				}

				return _signatureTimeStampGenTime;
			}
		}
		private DateTime? _signatureTimeStampGenTime;

		public DateTime? OldestArchiveTimeStampGenTime
		{
			get
			{
				if (_oldestArchiveTimeStampGenTime == null) {
					var timeStampNode = _sigNode.GetArchiveTimeStampNodes(_nsManager).FirstOrDefault()?.GetEncapsulatedTimeStampNode(_nsManager);
					_oldestArchiveTimeStampGenTime = GetTimeStampGenTime(timeStampNode);
				}

				return _oldestArchiveTimeStampGenTime;
			}
		}
		private DateTime? _oldestArchiveTimeStampGenTime;

		private DateTime? GetTimeStampGenTime(XmlNode timeStampNode)
		{
			return GetTimeStamp(timeStampNode)?.ToTimeStampToken().TimeStampInfo.GenTime;
		}

		public IEnumerable<ArchiveTimeStampValidationData> ArchiveTimeStampValidationData => GetArchiveTimeStampData();
	}
}
