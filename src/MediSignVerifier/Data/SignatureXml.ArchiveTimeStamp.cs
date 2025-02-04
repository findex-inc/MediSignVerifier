using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using SignatureVerifier.Data.XAdES;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXml
	{
		private List<ArchiveTimeStampValidationData> _archiveTimeStampData;

		private IEnumerable<ArchiveTimeStampValidationData> GetArchiveTimeStampData()
		{
			if (_archiveTimeStampData == null) {
				_archiveTimeStampData = new List<ArchiveTimeStampValidationData>();

				var timeStampNodes = _sigNode.GetArchiveTimeStampNodes(_nsManager);
				for (int idx = 0; idx < timeStampNodes.Count(); idx++) {
					var timeStampRootNode = timeStampNodes.ElementAt(idx);
					var id = timeStampRootNode.GetId();
					byte[] targetValue = null;
					byte[] timeStamp = null;
					string c14nMethod = null;
					CertificateData signerCert = null;
					TimeStampConvertException convertError = null;

					try {
						//タイムスタンプ対象データ
						var timeStampC14nNode = timeStampRootNode.GetCanonicalizationMethodNode(_nsManager);
						c14nMethod = timeStampC14nNode?.GetAlgorithm();
						targetValue = GetArchiveTimeStampTargetValue(id, c14nMethod);

						//TimeStamp（失敗したら例外Throw：続行不能のため）
						timeStamp = GetTimeStamp(timeStampRootNode.GetEncapsulatedTimeStampNode(_nsManager));

						//TSA証明書(失敗したら例外Throw：続行しても意味がない)
						signerCert = new CertificateData(
							timeStampRootNode?.GetAbsolutePath(),
							id,
							GetTimeStampSignerCertificate(timeStamp));
					}
					catch (TimeStampConvertException ex) {
						//後続のATSを処理するために退避
						convertError = ex;
					}

					var certErrors = new List<TimeStampConvertException>();

					//ValidData
					var validDataNode = _sigNode.GetTimeStampValidationDataNode(id, _nsManager);

					//Certificates
					IEnumerable<byte[]> certificates = null;
					try {
						certificates = GetTimeStampCertificates(validDataNode);
					}
					catch (TimeStampConvertException ex) {
						//続行可能
						certErrors.Add(ex);
					}

					//Revocations
					IEnumerable<byte[]> crls = null;
					try {
						crls = GetTimeStampCrls(validDataNode);
					}
					catch (TimeStampConvertException ex) {
						//続行可能
						certErrors.Add(ex);
					}

					IEnumerable<byte[]> ocsp = null;
					try {
						ocsp = GetTimeStampOcsps(validDataNode);
					}
					catch (TimeStampConvertException ex) {
						//続行可能
						certErrors.Add(ex);
					}

					var validationData = (validDataNode == null)
						? (CertificatePathValidationData)null
						: new CertificatePathValidationData(validDataNode?.GetAbsolutePath(), certificates, crls, ocsp);

					//タイムスタンプ対象データ
					_archiveTimeStampData.Add(new ArchiveTimeStampValidationData(id, idx,
						new TimeStampValidationData(id, timeStamp, c14nMethod, targetValue, signerCert, validationData, certErrors),
						convertError));
				}
			}

			return _archiveTimeStampData;
		}

		private const string _atsTargetValueGetErrorMsg = "の取得(正規化)に失敗しました。";

		private byte[] GetArchiveTimeStampTargetValue(string currentATSNodeId, string c14nMethod)
		{
			//以下の各要素を正規化し、連結していく(for v1.4.1)
			//途中で例外が発生したらThrow（続行不能のため）
			var totalValue = new List<byte>();

			//References
			AppendReferenceValue(totalValue);

			//SignedInfo
			AppendSignedInfoValue(c14nMethod, totalValue);

			//SignatureValue
			AppendSignatureValue(c14nMethod, totalValue);

			//KeyInfo
			AppendKeyInfoValue(c14nMethod, totalValue);

			//UnsignedSignatureProperties配下
			AppendUnsignedSignaturePropValue(c14nMethod, currentATSNodeId, totalValue);

			//Object要素
			AppendObjectValue(c14nMethod, totalValue);

			return totalValue.ToArray();
		}

		private void AppendReferenceValue(List<byte> totalValue)
		{
			try {
				//URIで指定されている要素が対象。ReferenceのTransformで指定されているアルゴリズムで正規化する
				//XPathが指定されているパターンもあり
				var refNodes = _sigNode.GetSignedInfoReferenceNodes(_nsManager);
				foreach (var refNode in refNodes.OfType<XmlNode>()) {
					var uri = refNode.GetUri();

					TransformChain chain;
					var transformsNode = refNode.GetTransformsNode(_nsManager);
					if (transformsNode == null) {
						//標準
						chain = CreateTransformChain(null);
					}
					else {
						chain = new TransformChain();
						chain.LoadXml(transformsNode as XmlElement);
					}

					var realNamespaces = ((XmlElement)refNode).GetAllNamespaces();
					chain.AddNamespaces(realNamespaces);

					if (string.IsNullOrEmpty(uri)) {
						totalValue.AddRange(chain.GetOutput(_doc).ToByteArray());
					}
					else {
						totalValue.AddRange(chain.GetOutput(_doc, uri).ToByteArray());
					}
				}
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"Reference要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}

		private void AppendSignedInfoValue(string c14nMethod, List<byte> totalValue)
		{
			try {
				var node = _sigNode.GetSignedInfoNode(_nsManager) as XmlElement;
				totalValue.AddRange(CreateTransformChain(c14nMethod).GetOutput(node, true).ToByteArray());
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"SignedInfo要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}

		private void AppendSignatureValue(string c14nMethod, List<byte> totalValue)
		{
			try {
				var node = _sigNode.GetSignatureValueNode(_nsManager) as XmlElement;
				totalValue.AddRange(CreateTransformChain(c14nMethod).GetOutput(node, true).ToByteArray());
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"SignatureValue要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}

		private void AppendKeyInfoValue(string c14nMethod, List<byte> totalValue)
		{
			try {
				if (_sigNode.GetKeyInfoNode(_nsManager) is XmlElement node) {
					totalValue.AddRange(CreateTransformChain(c14nMethod).GetOutput(node, true).ToByteArray());
				}
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"KeyInfo要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}

		private void AppendUnsignedSignaturePropValue(string c14nMethod, string currentId, List<byte> totalValue)
		{
			//自分以降は含めない（このIDの要素に到達したら終了)
			//※自分より古いアーカイブタイムスタンプは含まれる
			try {
				var unsignedPropNode = _sigNode.FindUnsignedSignaturePropertiesNode(_nsManager);
				foreach (var node in unsignedPropNode.ChildNodes.OfType<XmlElement>()) {

					if (node.GetId() == currentId) break;

					totalValue.AddRange(CreateTransformChain(c14nMethod).GetOutput(node, true).ToByteArray());
				}
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"非署名属性要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}

		private void AppendObjectValue(string c14nMethod, List<byte> totalValue)
		{
			//QualifyingPropertiesを持たないもの
			try {
				var objNodes = _sigNode.GetObjectNodes(_nsManager);
				foreach (var objNode in objNodes) {
					if (objNode.ChildNodes.OfType<XmlNode>().Any(n => n.LocalName == "QualifyingProperties")) {
						break;
					}

					totalValue.AddRange(CreateTransformChain(c14nMethod).GetOutput(objNode as XmlElement, true).ToByteArray());
				}
			}
			catch (Exception ex) {
				//続行不可
				throw new TimeStampConvertException(TimeStampItemNames.Target, $"Object要素{_atsTargetValueGetErrorMsg}", ex);
			}
		}
	}
}
