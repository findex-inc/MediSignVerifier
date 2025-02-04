using System;

namespace SignatureVerifier.Verifiers
{
	internal class CertificatePathValidationException : Exception
	{
		public CertificatePathValidationException(VerificationStatus status, string message, Exception innerException = null)
			: base(message, innerException)
		{
			Status = status;
		}

		public VerificationStatus Status { get; }

	}
}
