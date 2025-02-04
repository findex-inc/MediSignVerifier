using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Data.XAdES;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXml
	{
		private TimeStampValidationData GetSignatureTimeStampData()
		{
			if (_signatureTimeStampData == null) {

				var timeStampRootNode = _sigNode.GetSignatureTimeStampNode(_nsManager);

				//タイムスタンプ対象データ
				var targetNode = _sigNode.GetSignatureValueNode(_nsManager) as XmlElement;
				var timeStampC14nNode = timeStampRootNode?.GetCanonicalizationMethodNode(_nsManager);
				var c14nMethod = timeStampC14nNode?.GetAlgorithm();
				var targetValue = GetTimeStampTargetValue(targetNode, c14nMethod);

				//TimeStamp（失敗したら例外Throw：続行不能のため）
				var timeStampNode = timeStampRootNode?.GetEncapsulatedTimeStampNode(_nsManager);

				var id = timeStampNode?.ParentNode?.GetId();
				byte[] timeStamp = GetTimeStamp(timeStampNode);

				//TSA証明書(失敗したら例外Throw：続行しても意味がない)
				CertificateData signerCert = new CertificateData(
					timeStampNode?.ParentNode?.GetAbsolutePath(),
					id,
					GetTimeStampSignerCertificate(timeStamp));

				var certErrors = new List<TimeStampConvertException>();

				//ValidData
				var validDataNode = _sigNode.GetTimeStampValidationDataNode(id, _nsManager);

				//Certificates
				IEnumerable<byte[]> certificates = null;
				try {
					certificates = GetTimeStampCertificates(validDataNode);
				}
				catch (TimeStampConvertException ex) {
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

				_signatureTimeStampData =
					new TimeStampValidationData(id, timeStamp, c14nMethod, targetValue, signerCert, validationData, certErrors);
			}

			return _signatureTimeStampData;
		}
		private TimeStampValidationData _signatureTimeStampData;

		//---以下、タイムスタンプ共通

		private byte[] GetTimeStampTargetValue(XmlNode targetNode, string c14nMethod)
		{
			if (targetNode == null) throw new TimeStampConvertException(TimeStampItemNames.Target, "タイムスタンプ対象要素が見つかりませんでした。");

			try {
				var transformChain = CreateTransformChain(c14nMethod);
				return transformChain.GetOutput(targetNode as XmlElement, true).ToByteArray();
			}
			catch (Exception ex) {
				throw new TimeStampConvertException(TimeStampItemNames.Target, "タイムスタンプ対象要素の正規化に失敗しました。", ex);
			}
		}

		private byte[] GetTimeStamp(XmlNode timeStampNode)
		{
			if (timeStampNode == null) throw new TimeStampConvertException(TimeStampItemNames.Token, "タイムスタンプの要素が見つかりませんでした。");

			try {
				return timeStampNode.InnerText.ToBytes();
			}
			catch (Exception ex) {
				//検証続行不能
				throw new TimeStampConvertException(TimeStampItemNames.Token, "タイムスタンプのデコードに失敗しました。", ex);
			}
		}

		private byte[] GetTimeStampSignerCertificate(byte[] timeStamp)
		{
			try {
				return timeStamp.ToTimeStampToken().GetSignerCertificate()?.GetEncoded();
			}
			catch (Exception ex) {
				throw new TimeStampConvertException(TimeStampItemNames.TSACert, "TSA証明書の取得に失敗しました。", ex);
			}
		}

		private IEnumerable<byte[]> GetTimeStampCertificates(XmlNode validDataNode)
		{
			if (validDataNode == null) return Enumerable.Empty<byte[]>();

			var certNodes = validDataNode.GetCertificateValuesNodes(_nsManager);
			var certificates = new List<byte[]>();
			foreach (var certNode in certNodes) {
				try {
					certificates.Add(certNode.InnerText.ToBytes());
				}
				catch (Exception ex) {
					//1個でもエラーが発生したらパス検証は失敗するのでThrow
					throw new TimeStampConvertException(TimeStampItemNames.TSACert, "TSA証明書群のデコードに失敗しました。", ex);
				}
			}

			return certificates;
		}

		private IEnumerable<byte[]> GetTimeStampCrls(XmlNode validDataNode)
		{
			if (validDataNode == null) return Enumerable.Empty<byte[]>();

			var crlNodes = validDataNode.GetCRLValuesNodes(_nsManager);
			var crls = new List<byte[]>();
			foreach (var crlNode in crlNodes) {
				try {
					crls.Add(crlNode.InnerText.ToBytes());
				}
				catch (Exception ex) {
					//1個でもエラーが発生したらパス検証は失敗するのでThrow
					throw new TimeStampConvertException(TimeStampItemNames.TSACert, "TSA証明書失効情報群のデコードに失敗しました。", ex);
				}
			}

			return crls;
		}

		private IEnumerable<byte[]> GetTimeStampOcsps(XmlNode validDataNode)
		{
			if (validDataNode == null) return Enumerable.Empty<byte[]>();

			var crlNodes = validDataNode.GetOCSPValuesNodes(_nsManager);
			var crls = new List<byte[]>();
			foreach (var crlNode in crlNodes) {
				try {
					crls.Add(crlNode.InnerText.ToBytes());
				}
				catch (Exception ex) {
					//1個でもエラーが発生したらパス検証は失敗するのでThrow
					throw new TimeStampConvertException(TimeStampItemNames.TSACert, "TSA証明書失効情報群のデコードに失敗しました。", ex);
				}
			}

			return crls;
		}

		private TransformChain CreateTransformChain(string c14nMethod)
		{
			var transformChain = new TransformChain();
			if (!string.IsNullOrEmpty(c14nMethod)) {
				var transform = SecurityCryptoXmlHelper.CreateTransform(c14nMethod);
				if (transform != null) {
					transformChain.Add(transform);
				}
			}

			return transformChain;
		}
	}
}
