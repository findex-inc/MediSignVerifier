using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using SignatureVerifier.Data;

namespace SignatureVerifier.Reports.BouncyCastle
{
	internal class X509CRLRevocationInfo : ICertificateRevocationInfo
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		private readonly X509Certificate _cert;
		private readonly X509Crl _crl;

		public X509CRLRevocationInfo(string source, X509Certificate cert, X509Crl crl)
		{
			this.Source = source;

			_cert = cert;
			_crl = crl;
		}

		public CertificateRevocationDataType DataType => CertificateRevocationDataType.CRL;

		public string Source { get; }

		public string Issuer => _crl?.IssuerDN?.ToString();

		public DateTime? ThisUpdate => _crl.ThisUpdate;

		public DateTime? NextUpdate => _crl.NextUpdate?.Value;

		public ICertificateRevocationInfoEntry[] Entries => GetCrlDescription().ToArray();

		private IEnumerable<ICertificateRevocationInfoEntry> GetCrlDescription()
		{
			var entry = _crl.GetRevokedCertificate(_cert.SerialNumber);
			if (entry == null) {

				yield break;
			}

			yield return new Entry(entry);
		}

		private class Entry : ICertificateRevocationInfoEntry
		{
			private readonly X509CrlEntry _entry;

			public Entry(X509CrlEntry entry)
			{
				_entry = entry;
			}

			public string SerialNumber => _entry.SerialNumber?.ToString(16).PaddingHex();

			public DateTime? RevocationDate => _entry.RevocationDate;

			public CertificateRevocationReason Reason
			{
				get
				{
					var value = GetCrlReason()?.Value;
					if (value != null) {

						return CertificateRevocationReason.Parse(value.IntValue);
					}

					return null;
				}
			}

			private CrlReason GetCrlReason()
			{
				try {
					var bytes = _entry.GetExtensionValue(X509Extensions.ReasonCode);
					if (bytes == null) {

						return null;
					}

					var extensionValue = X509ExtensionUtilities.FromExtensionValue(bytes);
					return new CrlReason(DerEnumerated.GetInstance(extensionValue));
				}
				catch (Exception e) {

					_logger.Debug(e, "Ignore exceptions, return null.");

					return null;
				}
			}
		}
	}
}
