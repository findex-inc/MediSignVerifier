using System.Collections.Generic;
using System.Linq;
using SignatureVerifier.Data;
using SignatureVerifier.Data.BouncyCastle;
using SignatureVerifier.Reports.BouncyCastle;

namespace SignatureVerifier.Reports.Reporters
{
	internal class ArchiveTimeStampReporter
	{
		private readonly ReportConfig _config;

		public ArchiveTimeStampReporter(ReportConfig config)
		{
			_config = config;
		}

		public ArchiveTimeStampReportList Generate(ISignature signature, VerificationResult result)
		{
			//Index == -1 のものは、検証不能なエラーのはずで複数は出ない想定
			var commonError = result.ToReportItems(signature.SourceType, -1).FirstOrDefault();
			var reports = new List<ArchiveTimeStampReport>();

			if (signature.ArchiveTimeStampValidationData != null) {
				foreach (var ats in signature.ArchiveTimeStampValidationData) {
					var validData = ats.ValidationData;
					var timeStampData = validData?.TimeStampData;

					var resultItems = result.ToReportItems(signature.SourceType, ats.DispIndex);
					var status = result.GetStatus(signature.SourceType, ats.DispIndex);

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

					var report = ArchiveTimeStampReport.Create(
						ats.DispIndex,
						status,
						ats.Id,
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

					reports.Add(report);
				}
			}

			return ArchiveTimeStampReportList.Create(result.GetStatus(signature.SourceType), commonError?.Message, reports);
		}
	}
}
