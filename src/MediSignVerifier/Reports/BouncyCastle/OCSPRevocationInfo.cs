using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Reports.BouncyCastle
{
	internal class OCSPRevocationInfo : ICertificateRevocationInfo
	{
		private readonly X509Certificate _cert;
		private readonly OcspResp _ocsp;

		public OCSPRevocationInfo(string source, X509Certificate cert, OcspResp ocsp)
		{
			this.Source = source;

			_cert = cert;
			_ocsp = ocsp;
		}

		public CertificateRevocationDataType DataType => CertificateRevocationDataType.OCSP;

		public string Source { get; }

		public string Issuer => _cert.IssuerDN.ToString();

		public DateTime? ThisUpdate => _ocsp.GetResponseObject<BasicOcspResp>()
			?.Responses.FirstOrDefault()?.ThisUpdate;

		public DateTime? NextUpdate => _ocsp.GetResponseObject<BasicOcspResp>()
			?.Responses.FirstOrDefault()?.NextUpdate?.Value;

		public ICertificateRevocationInfoEntry[] Entries => GetCrlDescription().ToArray();

		private IEnumerable<ICertificateRevocationInfoEntry> GetCrlDescription()
		{
			var response = _ocsp.GetResponseObject<BasicOcspResp>()
				?.Responses.FirstOrDefault();

			if (response == null) {

				yield break;
			}

			var status = response.GetCertStatus();
			if (status == Org.BouncyCastle.Ocsp.CertificateStatus.Good) {

				yield break;
			}

			yield return new Entry(response);
		}

		private class Entry : ICertificateRevocationInfoEntry
		{
			private readonly SingleResp _resp;

			public Entry(SingleResp resp)
			{
				_resp = resp;
			}

			public string SerialNumber => _resp.GetCertID()?.SerialNumber?.ToString(16).PaddingHex();

			public DateTime? RevocationDate =>
				(_resp.GetCertStatus() as RevokedStatus)?.RevocationTime;

			public CertificateRevocationReason Reason
			{
				get
				{
					var status = _resp.GetCertStatus();
					if (status is UnknownStatus) {

						return CertificateRevocationReason.Unknown;
					}

					var value = GetCrlReason()?.Value;
					if (value != null) {

						return CertificateRevocationReason.Parse(value.IntValue);
					}

					return null;
				}
			}

			private CrlReason GetCrlReason()
			{
				if (_resp.GetCertStatus() is RevokedStatus revoked) {
					return new CrlReason(revoked.RevocationReason);
				}

				return null;
			}

		}

	}
}
