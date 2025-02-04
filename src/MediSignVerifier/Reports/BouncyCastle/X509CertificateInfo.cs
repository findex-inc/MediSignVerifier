using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.X509;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Reports.BouncyCastle
{
	internal class X509CertificateInfo : ICertificateInfo
	{
		private readonly X509Certificate _cert;
		private readonly DateTime _validDate;
		private readonly IEnumerable<ICertificateRevocationInfo> _revocations;

		public X509CertificateInfo(string source, X509Certificate cert, DateTime validDate, IEnumerable<ICertificateRevocationInfo> revocations)
		{
			this.Source = source;

			_cert = cert;
			_validDate = validDate;
			_revocations = revocations;
		}

		public CertificateRevocationStatus CertStatus => GetCertStatus();

		private CertificateRevocationStatus GetCertStatus()
		{
			if (_revocations?.Any() ?? false) {

				if (!_revocations.Any(x => x.Entries.Any())) {

					return CertificateRevocationStatus.UNREVOKED;
				}

				if (_revocations.Any(x => x.Entries.Any(e => IsRevoked(e)))) {

					return CertificateRevocationStatus.REVOKED;
				}

				return CertificateRevocationStatus.UNDETERMINED;

			}
			else {

				if (_cert.IsSelfSigned()) {

					return CertificateRevocationStatus.UNREVOKED;
				}

				return CertificateRevocationStatus.UNDETERMINED;

			}

		}

		private bool IsRevoked(ICertificateRevocationInfoEntry entry)
		{
			var reasonCodeValue = entry.Reason;

			if (reasonCodeValue == CertificateRevocationReason.RemoveFromCrl) {

				return false;
			}

			if (_validDate.Ticks < entry.RevocationDate?.Ticks) {
				switch (reasonCodeValue) {
					case var _ when reasonCodeValue == CertificateRevocationReason.Unspecified:
					case var _ when reasonCodeValue == CertificateRevocationReason.KeyCompromise:
					case var _ when reasonCodeValue == CertificateRevocationReason.CACompromise:
					case var _ when reasonCodeValue == CertificateRevocationReason.AACompromise:
						return true;

					default:
						return false;
				}
			}

			return true;
		}

		public string Source { get; }

		public string SerialNumber => _cert?.SerialNumber?.ToString(16).PaddingHex();

		public string Issuer => _cert?.IssuerDN?.ToString();

		public string Subject => _cert?.SubjectDN?.ToString();

		public DateTime? NotBefore => _cert?.NotBefore;

		public DateTime? NotAfter => _cert?.NotAfter;

		public bool? IsCa => (_cert.GetBasicConstraints() != -1) ? true : (bool?)null;

		public string HcRole => _cert?.GetHcRoles().FirstOrDefault()?.RoleName;

		public string[] ExtendedKeyUsage => _cert?.GetExtendedKeyUsageNames()?.ToArrayOrNull();

		public ICertificateRevocationInfo[] Revocations => _revocations?.ToArrayOrNull();

	}
}
