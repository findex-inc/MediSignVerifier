using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SignatureVerifier.Reports;
using SignatureVerifier.Reports.Reporters;
using SignatureVerifier.Verifiers;
using SignatureVerifier.Verifiers.StructureVerifiers;

namespace SignatureVerifier
{
	/// <summary>
	/// 検証結果レポート生成クラス
	/// </summary>
	public class SignatureReporter
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		private readonly ReportConfig _config;

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="config">レポートオプション</param>
		public SignatureReporter(ReportConfig config)
		{
			_config = config;
		}

		/// <summary>
		/// レポート作成処理を実行する
		/// </summary>
		/// <param name="doc">検証対象ドキュメント</param>
		/// <param name="verificationTime">検証時刻</param>
		/// <param name="result">検証結果</param>
		/// <returns>検証結果レポートデータ</returns>
		public SignatureVerificationReport Generate(
			ISignedDocument doc,
			DateTime verificationTime,
			SignatureVerificationResult result
			)
		{
			try {

				var report = new SignatureVerificationReport(doc.FilePath, DateTime.Now, verificationTime, result.Status, doc.DocumentType);

				var signatureCount = doc.Signatures.Count();
				if (signatureCount == 0) {
					//署名要素が一つもない場合（結果は強制的にINVALID）※続行不可
					report.Message = "検証できる署名が存在しませんでした。";
					return report;
				}

				//署名構造
				var structureReport = GetStructureReport(SignatureSourceType.None, result);

				//スキーマエラー
				if (structureReport.Status != VerificationStatus.VALID) {
					//続行不可
					report.Structure = structureReport;
					report.Message = "署名構造の検証に失敗しました。";
					return report;
				}

				//検証結果メッセージ
				var otherSignatureCount = doc.Signatures.Count(m => m.SourceType == SignatureSourceType.Unknown);

				if (otherSignatureCount == signatureCount) {
					//規定の位置に署名要素が無い場合（結果は強制的にINVALID）
					report.Message = "規定の署名が存在しませんでした。";
				}
				else if (otherSignatureCount > 0) {
					//規定の位置以外にも署名要素がある場合（結果は検証結果に準拠）
					report.Message = "規定以外の署名が存在します。";
				}
				else {
					report.Message = GetResultMessage(result.Status);
				}

				var sigReportList = new List<SignatureReportSet>();
				for (int idx = 0; idx < doc.Signatures.Count(); idx++) {
					var signature = doc.Signatures.ElementAt(idx);

					var sigReport = new SignatureReportSet(idx + 1, signature.SourceType, signature.ESLevel, signature.Path)
					{
						//署名構造
						Structure = GetStructureReport(signature.SourceType, result),
					};
					sigReportList.Add(sigReport);

					if (sigReport.Structure.Status != VerificationStatus.VALID) {
						//XAdES構造検証で失敗しているので、個別レポートは出力しない？
						continue;
					}

					//署名者証明書
					sigReport.SigningCertificate = GetSigningCertificateReport(signature, verificationTime, result);

					//参照データ
					sigReport.References = GetReferenceReports(signature, result);

					//署名データ
					sigReport.SignatureValue = GetSignatureReport(signature, result);

					//署名タイムスタンプ
					if (signature.ESLevel >= ESLevel.T) {
						sigReport.SignatureTimeStamp = GetSignatureTimeStampReport(signature, result);
					}

					//アーカイブタイムスタンプ
					if (signature.ESLevel == ESLevel.A) {
						sigReport.ArchiveTimeStamps = GetArchiveTimeStampReports(signature, result);
					}
				}

				report.Signatures = sigReportList.ToArray();
				return report;
			}
			catch (Exception ex) {
				_logger.Error(ex);
				throw;
			}
		}

		private string GetResultMessage(VerificationStatus status)
		{
			switch (status) {
				case VerificationStatus.VALID:
					return "署名検証に成功しました。";
				case VerificationStatus.INVALID:
					return "署名検証に失敗しました。";
				case VerificationStatus.INDETERMINATE:
					return "署名検証に必要な情報が不足しています。";
				default:
					return null;
			}
		}

		private StructureReport GetStructureReport(SignatureSourceType sourceType, SignatureVerificationResult result)
		{
			return new StructureReporter(_config)
				.Generate(sourceType, result.GetVerificationResult(AggregateStructureVerifier.Name));
		}

		private SigningCertificateReport GetSigningCertificateReport(ISignature signature,
			DateTime verificationTime,
			SignatureVerificationResult result)
		{
			return new SigningCertificateReporter(_config)
				.Generate(signature, verificationTime, result.GetVerificationResult(SigningCertificateVerifier.Name));
		}

		private ReferenceReportList GetReferenceReports(ISignature signature,
			SignatureVerificationResult result)
		{
			return new ReferenceReporter(_config)
				.Generate(signature, result.GetVerificationResult(ReferenceVerifier.Name));
		}

		private SignatureValueReport GetSignatureReport(ISignature signature,
			SignatureVerificationResult result)
		{
			return new SignatureValueReporter(_config)
				.Generate(signature, result.GetVerificationResult(SignatureValueVerifier.Name));
		}

		private SignatureTimeStampReport GetSignatureTimeStampReport(ISignature signature,
			SignatureVerificationResult result)
		{
			return new SignatureTimeStampReporter(_config)
				.Generate(signature, result.GetVerificationResult(SignatureTimeStampVerifier.Name));
		}

		private ArchiveTimeStampReportList GetArchiveTimeStampReports(ISignature signature,
			SignatureVerificationResult result)
		{
			return new ArchiveTimeStampReporter(_config)
				.Generate(signature, result.GetVerificationResult(ArchiveTimeStampVerifier.Name));
		}
	}
}
