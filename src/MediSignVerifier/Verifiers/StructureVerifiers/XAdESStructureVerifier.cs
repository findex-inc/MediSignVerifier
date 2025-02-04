using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NLog;
using SignatureVerifier.Data.XAdES;

namespace SignatureVerifier.Verifiers.StructureVerifiers
{
	/// <summary>
	/// XAdES構造検証器
	/// </summary>
	internal class XAdESStructureVerifier : IStructureVerifier
	{
		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

#pragma warning disable IDE0052
		private readonly VerificationConfig _config;
#pragma warning restore IDE0052
		private readonly IList<VerificationResultItem> _results;

		private XmlNamespaceManager _nsManager;

		public XAdESStructureVerifier(VerificationConfig config)
		{
			_config = config;
			_results = new List<VerificationResultItem>();
		}

		public VerificationResult Verify(ISignedDocument doc)
		{
			_logger.Trace($"XAdES構造検証を開始します。");

			var xmlDoc = doc.Raw as XmlDocument;
			_nsManager = xmlDoc.CreateXAdESNamespaceManager();

			foreach (XmlNode signatureNode in xmlDoc.SelectNodes("//xs:Signature", _nsManager)) {

				VerifySignature(signatureNode);
			}

			var status = _results.Select(x => x.Status).ToConclusion();
			_logger.Debug($"XAdES構造検証を終了します。Status={status}");

			return new VerificationResult(status, _results);
		}


		private void VerifySignature(XmlNode signatureNode)
		{
			VerifyMandatoryNodes(signatureNode);

			VerifySignedPropertiesNode(signatureNode);

			return;
		}


		private void VerifyMandatoryNodes(XmlNode signatureNode)
		{
			const string itemName = "XAdES必須要素";

			var sourceType = signatureNode.GetSignatureSourceType();
			var level = signatureNode.GetESLevel(_nsManager);

			var checkingXPaths = new[]
			{
				"xs:SignedInfo",
				"xs:SignedInfo/xs:CanonicalizationMethod",
				"xs:SignedInfo/xs:SignatureMethod",
				"xs:SignedInfo/xs:Reference",
				"xs:SignedInfo/xs:Reference/xs:DigestMethod",
				"xs:SignedInfo/xs:Reference/xs:DigestValue",
				"xs:SignatureValue",
				"xs:Object",
				"xs:Object/xa:QualifyingProperties",
				"xs:Object/xa:QualifyingProperties/xa:SignedProperties",
				"xs:Object/xa:QualifyingProperties/xa:SignedProperties/xa:SignedSignatureProperties",
			};

			if (level >= ESLevel.T) {
				var checkingXPathsLevelT = new[]
				{
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/xa:SignatureTimeStamp",
				};
				checkingXPaths = checkingXPaths.Concat(checkingXPathsLevelT).ToArray();
			}

			if (level >= ESLevel.A) {
				var checkingXPathsLevelA = new[]
				{
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/xa:CertificateValues",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/xa:RevocationValues",
					"xs:Object/xa:QualifyingProperties/xa:UnsignedProperties/xa:UnsignedSignatureProperties/xa141:ArchiveTimeStamp",
				};
				checkingXPaths = checkingXPaths.Concat(checkingXPathsLevelA).ToArray();
			}

			var status = VerificationStatus.VALID;

			var id = signatureNode.GetId();
			if (string.IsNullOrEmpty(id)) {
				status = VerificationStatus.INVALID;
				DoPostVerification(status, sourceType, itemName, "Signature要素のId属性が見つかりません。");
			}


			foreach (var xpath in checkingXPaths) {
				var node = signatureNode.SelectSingleNode(xpath, _nsManager);
				if (node == null) {
					status = VerificationStatus.INVALID;
					DoPostVerification(status, sourceType, itemName, $"\"{xpath}\"要素が見つかりません。");
				}
			}

			var nodeKeyInfo = signatureNode.SelectSingleNode("xs:KeyInfo", _nsManager);
			if (nodeKeyInfo == null) {
				var nodeSigningCertificateV2 = signatureNode.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:SignedProperties/xa:SignedSignatureProperties/xa:SigningCertificateV2", _nsManager);
				var nodeSigningCertificate = signatureNode.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:SignedProperties/xa:SignedSignatureProperties/xa:SigningCertificate", _nsManager);
				if (nodeSigningCertificateV2 == null && nodeSigningCertificate == null) {
					status = VerificationStatus.INVALID;
					DoPostVerification(status, sourceType, itemName, "xs:KeyInfoもしくはxa:SigningCertificateV2(xa:SigningCertificate)要素が見つかりません。");
				}
			}

			if (status == VerificationStatus.VALID) {
				DoPostVerification(status, sourceType, itemName, null);
			}

			return;
		}

		private void VerifySignedPropertiesNode(XmlNode signatureNode)
		{
			const string itemName = "SignedProperties要素";

			var sourceType = signatureNode.GetSignatureSourceType();

			var status = VerificationStatus.VALID;

			var nodeProperties = signatureNode.SelectSingleNode("xs:Object/xa:QualifyingProperties/xa:SignedProperties", _nsManager);
			var nodePropertiesId = nodeProperties.GetId()?.Insert(0, "#");
			var nodeReferenceList = signatureNode.SelectNodes("xs:SignedInfo/xs:Reference", _nsManager);
			foreach (XmlElement nodeReference in nodeReferenceList) {
				if (nodeReference.GetAttribute("URI") == nodePropertiesId) {
					var type = nodeReference.GetAttribute("Type");
					if (!Regex.IsMatch(type, @"http://uri.etsi.org/01903#SignedProperties", RegexOptions.Compiled)) {
						status = VerificationStatus.INVALID;
						DoPostVerification(status, sourceType, itemName, "正しいType属性がセットされていません。");
					}
				}
			}

			if (status == VerificationStatus.VALID) {
				DoPostVerification(status, sourceType, itemName, null);
			}

			return;
		}

		private void DoPostVerification(VerificationStatus status, SignatureSourceType type, string itemName, string message)
		{
			var item = new VerificationResultItem(status, type, AggregateStructureVerifier.Name, itemName, mappedItem: null, message);

			_results.Add(item);

			if (item.Status != VerificationStatus.VALID) {

				_logger.Warn(item.Message);

				VerifiedEvent?.Invoke(this, new VerifiedEventArgs(item.Status, $"{item.Source}：{item.MappedItem}", item.Message));
			}

			return;
		}
	}
}
