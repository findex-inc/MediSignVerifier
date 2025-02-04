using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using SignatureVerifier.Data.XAdES;
using SignatureVerifier.Security.Cryptography.Xml;

namespace SignatureVerifier.Data
{
	internal partial class SignatureXml
	{
		private SignatureValueValidationData GetSignatureValue()
		{
			if (_signatureData == null) {
				var sigValueNode = _sigNode.GetSignatureValueNode(_nsManager);
				var signedInfoNode = _sigNode.GetSignedInfoNode(_nsManager) as XmlElement;

				var id = sigValueNode?.GetId();
				var signatureMethod = signedInfoNode?.GetSignatureMethodNode(_nsManager)?.GetAlgorithm();
				var c14nMethod = signedInfoNode?.GetCanonicalizationMethodNode(_nsManager)?.GetAlgorithm();
				byte[] signatureValue = null;
				byte[] targetValue = null;
				Exception sigEx = null;

				try {
					signatureValue = sigValueNode.InnerText.ToBytes();
				}
				catch (Exception ex) {
					sigEx = new Exception("署名値のデコードに失敗しました。", ex);
				}

				if (sigEx == null) {
					try {
						var transform = SecurityCryptoXmlHelper.CreateTransform(c14nMethod);
						if (transform == null) {
							sigEx = new Exception("正規化アルゴリズムが不正です。");
						}
						else {
							var transformChain = new TransformChain();
							transformChain.Add(transform);
							using (var stream = (MemoryStream)transformChain.GetOutput(signedInfoNode, true)) {
								targetValue = stream.ToArray();
							}
						}
					}
					catch (Exception ex) {
						sigEx = new Exception("SignedInfo要素の正規化に失敗しました。", ex);
					}
				}

				_signatureData = new SignatureValueValidationData(id, c14nMethod, signatureMethod, signatureValue, targetValue, sigEx);
			}

			return _signatureData;
		}
		private SignatureValueValidationData _signatureData;
	}
}
