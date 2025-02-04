using System.Linq;
using System.Threading;
using SignatureVerifier.Data.XAdES;

namespace SignatureVerifier.Data
{
	partial class SignatureXml
	{

		/// <inheritdoc/>
		public ISigningCertificateValidationData SigningCertificateValidationData
		{
			get
			{
				if (_signingCertificateValidationData == null) {

					var instance = CreateInstance();
					Interlocked.CompareExchange(ref _signingCertificateValidationData, instance, null);
				}
				return _signingCertificateValidationData;

				ISigningCertificateValidationData CreateInstance()
				{
					var properties = _sigNode.FindUnsignedSignaturePropertiesNode(_nsManager);

					var keyInfo = _sigNode.FindReferencedKeyInfoNode(_nsManager);
					if (keyInfo != null) {

						if (keyInfo.GetX509CertificateNodes(_nsManager).Any()) {

							return new KeyInfoSigningCertificateValidationData(keyInfo, properties, _nsManager);
						}

						if (keyInfo.GetX509IssuerSerialNodes(_nsManager).Any()) {

							return new KeyInfoSigningCertificateValidationData(keyInfo, properties, _nsManager);
						}
					}

					var signingCertificateV2 = _sigNode.FindSigningCertificateV2Node(_nsManager);
					if (signingCertificateV2 != null) {

						return new SigningCertificateV2ValidationData(signingCertificateV2, properties, _nsManager);
					}

					var signingCertificate = _sigNode.FindSigningCertificateNode(_nsManager);
					if (signingCertificate != null) {

						return new SigningCertificateValidationData(signingCertificate, properties, _nsManager);
					}

					return null;
				}
			}
		}
		private ISigningCertificateValidationData _signingCertificateValidationData;


	}
}
