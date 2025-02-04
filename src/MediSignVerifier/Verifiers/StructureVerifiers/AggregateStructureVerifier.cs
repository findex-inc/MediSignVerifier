using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace SignatureVerifier.Verifiers.StructureVerifiers
{
	internal class AggregateStructureVerifier : IStructureVerifier
	{
		public static readonly string Name = "署名構造";

		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<VerifiedEventArgs> VerifiedEvent;

#pragma warning disable IDE0052
		private readonly VerificationConfig _config;
#pragma warning restore IDE0052
		private readonly IEnumerable<IStructureVerifier> _verifiers;
		private readonly bool _isContinueIfInvalid;

		public AggregateStructureVerifier(VerificationConfig config, IEnumerable<IStructureVerifier> verifiers, bool isContinueIfInvalid = false)
		{
			_config = config;
			_verifiers = verifiers;
			_isContinueIfInvalid = isContinueIfInvalid;

			foreach (var verifier in _verifiers) {

				verifier.VerifiedEvent += OnVerifiedEvent;
			}
		}


		public VerificationResult Verify(ISignedDocument doc)
		{
			_logger.Trace($"構造検証を開始します。");

			var results = new List<VerificationResult>();

			foreach (var verifier in _verifiers) {

				var verified = verifier.Verify(doc);
				results.Add(verified);

				if (!_isContinueIfInvalid) {
					if (verified.Status != VerificationStatus.VALID) {
						_logger.Warn($"VALID以外を検出したので中断します。");
						break;
					}
				}
			}

			var status = results.Select(x => x.Status).ToConclusion();
			var items = results.SelectMany(x => x.Items);

			_logger.Debug($"構造検証を終了します。Status={status}");

			return new VerificationResult(status, items);
		}


		private void OnVerifiedEvent(object sender, VerifiedEventArgs e)
		{
			if (e.Status != VerificationStatus.VALID) {

				VerifiedEvent?.Invoke(this, e);
			}
		}

	}
}
