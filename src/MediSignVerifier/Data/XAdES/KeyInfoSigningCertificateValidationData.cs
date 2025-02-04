using System.Linq;
using System.Threading;
using System.Xml;
using Org.BouncyCastle.Math;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Data.XAdES
{
	internal class KeyInfoSigningCertificateValidationData : ISigningCertificateValidationData
	{
		private readonly XmlNode _keyInfo;
		private readonly XmlNode _properties;
		private readonly XmlNamespaceManager _nsManager;

		public KeyInfoSigningCertificateValidationData(XmlNode keyInfo, XmlNode properties, XmlNamespaceManager nsManager)
		{
			_keyInfo = keyInfo;
			_properties = properties;
			_nsManager = nsManager;
		}


		public string Referenced => _keyInfo?.LocalName;


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
					var certElem = _keyInfo?.GetX509CertificateNodes(_nsManager).OfType<XmlElement>()
						.FirstOrDefault();
					if (certElem != null) {

						var id = _keyInfo?.GetId();
						var cert = certElem.InnerText?.ToBytes();
						return new CertificateData(_keyInfo.GetAbsolutePath(), id, cert);
					}

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


		public CertificateDigestData CertDigest => null;


		public CertificateIssuerSerialData IssuerSerial
		{
			get
			{
				if (_issuerSerial == null) {

					var instance = CreateInstance();
					Interlocked.CompareExchange(ref _issuerSerial, instance, null);
				}

				return _issuerSerial;

				CertificateIssuerSerialData CreateInstance()
				{
					var issuerSerialElem = _keyInfo?.GetX509IssuerSerialNodes(_nsManager).OfType<XmlElement>()
						.FirstOrDefault();
					if (issuerSerialElem != null) {

						var id = _keyInfo?.GetId();
						var issuerName = issuerSerialElem.GetX509IssuerNameNode(_nsManager)?.InnerText;

						//<element name="X509SerialNumber" type="integer"/>
						var serialNumberText = issuerSerialElem.GetX509SerialNumberNode(_nsManager)?.InnerText;
						var serialNumberValue = serialNumberText == null ? null : new BigInteger(serialNumberText.Trim(), 10);
						var serialNumber = serialNumberValue?.ToString(16).PaddingHex();

						return new CertificateIssuerSerialData(id, issuerName, serialNumber);
					}

					return null;
				}
			}
		}
		private CertificateIssuerSerialData _issuerSerial;


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
