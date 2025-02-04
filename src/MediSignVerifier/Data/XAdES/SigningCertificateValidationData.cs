using System.Linq;
using System.Threading;
using System.Xml;
using Org.BouncyCastle.Math;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Data.XAdES
{
	internal class SigningCertificateValidationData : ISigningCertificateValidationData
	{
		private readonly XmlNode _signingCertificate;
		private readonly XmlNode _properties;
		private readonly XmlNamespaceManager _nsManager;

		public SigningCertificateValidationData(XmlNode signingCertificate, XmlNode properties, XmlNamespaceManager nsManager)
		{
			_signingCertificate = signingCertificate;
			_properties = properties;
			_nsManager = nsManager;
		}


		public string Referenced => _signingCertificate?.LocalName;


		public CertificateData Certificate
		{
			get
			{
				if (_certificate == null) {

					var instance = CreateInstance();
					Interlocked.CompareExchange(ref _certificate, instance, null);
				}

				return _certificate;

				CertificateData CreateInstance()
				{
					if (IssuerSerial != null && _properties != null) {

						var issuer = IssuerSerial.IssuerName?.ToX509Name();
						var serial = IssuerSerial.SerialNumber?.ToBigInteger();

						var found = _properties.GetCertificateValuesNodes(_nsManager)
							.Select((x, i) => new
							{
								Certificate = x.InnerText?.ToBytes()?.ToX509CertificateOrDefault(),
								Index = i
							})
							.FirstOrDefault(x => x.Certificate.IssuerDN.Equivalent(issuer, inOrder: true) &&
								x.Certificate.SerialNumber.Equals(serial));

						if (found != null) {

							return new CertificateData(
								_properties.GetAbsolutePath(),
								$"xa:CertificateValues/xa:EncapsulatedX509Certificate[{found.Index}]",
								found.Certificate.GetEncoded());
						}

						// For verification of matching serial number.
						var sameIssuer = _properties.GetCertificateValuesNodes(_nsManager)
							.Select((x, i) => new
							{
								Certificate = x.InnerText?.ToBytes()?.ToX509CertificateOrDefault(),
								Index = i
							})
							.FirstOrDefault(x => x.Certificate.IssuerDN.Equivalent(issuer, inOrder: false));

						if (sameIssuer != null) {

							return new CertificateData(
								_properties.GetAbsolutePath(),
								$"xa:CertificateValues/xa:EncapsulatedX509Certificate[{sameIssuer.Index}]",
								sameIssuer.Certificate.GetEncoded());
						}
					}

					return null;
				}
			}
		}
		private CertificateData _certificate;


		public CertificateDigestData CertDigest
		{
			get
			{
				if (_certificateDigestData == null) {

					var instance = CreateInstance();
					Interlocked.CompareExchange(ref _certificateDigestData, instance, null);
				}

				return _certificateDigestData;

				CertificateDigestData CreateInstance()
				{
					var certDigest = _signingCertificate.GetCertDigestNode(_nsManager);
					if (certDigest != null) {

						var id = _signingCertificate?.GetAncestorId();
						var algorithm = certDigest.GetDigestMethodNode(_nsManager)?.GetAlgorithm();
						var digestValue = certDigest.GetDigestValueNode(_nsManager)?.InnerText?.ToBytes();

						return new CertificateDigestData(id, algorithm, digestValue);
					}

					return null;
				}
			}
		}
		private CertificateDigestData _certificateDigestData;


		public CertificateIssuerSerialData IssuerSerial
		{
			get
			{
				if (_issuerSerial == null) {

					var instance = CreateCertificateIssuerSerialData(_signingCertificate, _nsManager);
					Interlocked.CompareExchange(ref _issuerSerial, instance, null);
				}

				return _issuerSerial;
			}
		}
		private CertificateIssuerSerialData _issuerSerial;

		protected virtual CertificateIssuerSerialData CreateCertificateIssuerSerialData(XmlNode signingCertificate, XmlNamespaceManager nsManage)
		{
			var issuerSerialElem = signingCertificate.GetIssuerSerialNode(nsManage);
			if (issuerSerialElem != null) {

				var id = _signingCertificate?.GetAncestorId();
				var issuerName = issuerSerialElem.GetX509IssuerNameNode(nsManage)?.InnerText;

				//<element name="X509SerialNumber" type="integer"/>
				var serialNumberText = issuerSerialElem.GetX509SerialNumberNode(_nsManager)?.InnerText;
				var serialNumberValue = serialNumberText == null ? null : new BigInteger(serialNumberText.Trim(), 10);
				var serialNumber = serialNumberValue?.ToString(16).PaddingHex();

				return new CertificateIssuerSerialData(id, issuerName, serialNumber.ToString());
			}

			return null;
		}


		public CertificatePathValidationData PathValidationData
		{
			get
			{
				if (_pathValidationData == null) {

					if (_properties != null) {

						var instance = CreateInstance();
						Interlocked.CompareExchange(ref _pathValidationData, instance, null);
					}
				}

				return _pathValidationData;

				CertificatePathValidationData CreateInstance()
				{
					var certs = _properties.GetCertificateValuesNodes(_nsManager)
						.Select(x => x.InnerText?.ToBytes());
					var crls = _properties.GetCRLValuesNodes(_nsManager)
						.Select(x => x.InnerText?.ToBytes());
					var ocsps = _properties.GetOCSPValuesNodes(_nsManager)
						.Select(x => x.InnerText?.ToBytes());

					return new CertificatePathValidationData(_properties.GetAbsolutePath(), certs, crls, ocsps);
				}
			}
		}
		private CertificatePathValidationData _pathValidationData;

	}
}
