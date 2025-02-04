using System;
using System.Linq;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Reports.BouncyCastle;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SigningCertificateReporter
	{
		private readonly ReportConfig _config;

		public SigningCertificateReporter(ReportConfig config)
		{
			_config = config;
		}

		public SigningCertificateReport Generate(
			ISignature signature,
			DateTime verificationTime,
			VerificationResult result
			)
		{
			var sourceType = signature.SourceType;
			var items = result?.Items?.Where(x => x.Type == sourceType);
			var status = items?.Select(x => x.Status).ToConclusion() ?? VerificationStatus.INDETERMINATE;

			var baseTime = (signature.ESLevel < ESLevel.T)
				? verificationTime.ToUniversalTime()
				: signature.SignatureTimeStampGenTime ?? verificationTime.ToUniversalTime();

			var cert = signature.SigningCertificateValidationData?.Certificate;

			return SigningCertificateReport.Create(
				status: status,
				id: cert?.Id,
				certificateBaseTime: baseTime,
				resultItems: items?.Select(x => x.ToReport()),
				certificates: signature.SigningCertificateValidationData?.PathValidationData?.ToReportCertificates(baseTime.ToUniversalTime(), cert, _config)
					?? new[] { cert?.Value?.ToX509CertificateOrDefault()?.ToCertificateInfo(cert.Source, baseTime) }
				);
		}
	}
}
