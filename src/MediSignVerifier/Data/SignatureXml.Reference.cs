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
		private IEnumerable<ReferenceValidationData> GetReferenceData()
		{
			if (_referenceData == null) {
				_referenceData = new List<ReferenceValidationData>();
				var refNodes = _sigNode.GetSignedInfoReferenceNodes(_nsManager);

				for (int idx = 0; idx < refNodes.Count(); idx++) {
					//アルゴリズムが不正だったりするとReferenceの作成で例外が出るので、
					//ValidationDataへの変換は直接XMLから取得した値を使用する
					var refNode = refNodes.ElementAt(idx);
					var id = refNode.GetId();
					var uri = refNode.GetUri();
					string xPath = null;

					//Transform要素(子要素にXPathが指定されるケースがある)
					var transformNodes = refNode.GetTransformNodes(_nsManager);
					string transform = null;
					foreach (var tNode in transformNodes) {
						var xPathNode = tNode.GetXPathNode(_nsManager);
						if (xPathNode != null) {
							xPath = xPathNode.InnerText;
							continue;
						}

						transform = tNode.GetAlgorithm();
					}

					var digestMethod = refNode.GetDigestMethodNode(_nsManager)?.GetAlgorithm();
					var digestValueText = refNode.GetDigestValueNode(_nsManager)?.InnerText;
					byte[] digestValue = null;
					byte[] calculatedDigest = null;
					Exception refEx = null;

					try {
						digestValue = digestValueText.ToBytes();
					}
					catch (Exception ex) {
						refEx = new Exception("DigestValueのデコードに失敗しました。", ex);
					}

					if (refEx == null) {
						try {
							var reference = new Reference(uri);
							reference.LoadXml(refNode as XmlElement);
							reference.SetSignedXml(new SignedXml(_doc));
							calculatedDigest = reference.CalculateDigest(_doc, (_sigNode as XmlElement).GetAllNamespaces());
						}
						catch (Exception ex) {
							//Microsoft側の例外がいろいろ飛んできそうなのでExceptionでキャッチ。
							refEx = new Exception("ハッシュ値の計算に失敗しました。", ex);
						}
					}

					var refTarget = string.IsNullOrEmpty(uri) ? xPath : uri;
					_referenceData.Add(new ReferenceValidationData(id, idx, refTarget, transform, digestMethod, digestValue, calculatedDigest, refEx));
				}
			}

			return _referenceData;
		}
		private List<ReferenceValidationData> _referenceData;
	}
}
