using System;
using System.Collections.Generic;

namespace SignatureVerifier.Verifiers
{
	internal class TimeStampValidationException : Exception
	{
		public TimeStampValidationException(string itemName, string message, Exception innerException = null)
			: base(message, innerException)
		{
			Status = VerificationStatus.INVALID;
			ItemName = itemName;
		}

		public TimeStampValidationException(VerificationStatus status, string itemName, string message, Exception innerException = null)
			: this(itemName, message, innerException)
		{
			Status = status;
		}

		/// <summary>
		/// 検証結果
		/// </summary>
		public VerificationStatus Status { get; }

		/// <summary>
		/// 検証要件
		/// </summary>
		public string ItemName { get; }

	}

	internal class TimeStampValidationExceptions : List<TimeStampValidationException>
	{

	}
}
