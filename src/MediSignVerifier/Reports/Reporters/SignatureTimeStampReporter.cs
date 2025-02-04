using System.Linq;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Reports.BouncyCastle;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SignatureTimeStampReporter
	{
		private readonly ReportConfig _config;

		public SignatureTimeStampReporter(ReportConfig config)
		{
			_config = config;
		}

		public SignatureTimeStampReport Generate(
			ISignature signature,
			VerificationResult result
			)
		{
			var validData = signature.SignatureTimeStampValidationData;
			var timeStampData = validData?.TimeStampData;

			var resultItems = result.ToReportItems(signature.SourceType);
			var status = result.GetStatus(signature.SourceType);

			TsaSignatureInfo sigInfo = null;
			TsaCertificateInfo certInfo = null;
			ICertificateInfo[] certificates = null;

			if (timeStampData != null) {
				sigInfo = TsaSignatureInfo.Create(timeStampData);

				var tsaCert = validData.TsaCertificate;
				var validDate = timeStampData.ValidDate;

				try {
					//ToArrayのタイミングで処理が実行され例外が発生するのでここで確定しておく
					certificates = validData.CertificateValidationData?.ToReportCertificates(validDate, tsaCert, _config).ToArray()
						?? new[] { tsaCert?.Value?.ToX509CertificateOrDefault()?.ToCertificateInfo(tsaCert?.Source, validDate) }
						; //既にUTCになっている
				}
				catch {
					//レポート作成でパス検証エラーが出るが、既に検証を通しているので無視。
				}
				certInfo = TsaCertificateInfo.Create(timeStampData, certificates);
			}

			return SignatureTimeStampReport.Create(
				status,
				validData?.Id,
				timeStampData?.ValidDate,
				timeStampData?.GenTime,
				validData?.CanonicalizationMethod?.C14nAlgorithmName(),
				timeStampData?.HashAlgorithm,
				timeStampData?.HashValue?.ToBase64String(),
				timeStampData?.CalculatedValue?.ToBase64String(),
				sigInfo,
				certInfo,
				resultItems
				);
		}
	}
}
