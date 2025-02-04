using System;

namespace SignatureVerifier
{
	/// <summary>
	/// 検証イベントデータ
	/// </summary>
	public class VerifiedEventArgs : EventArgs
	{
		/// <summary>
		/// 検証結果
		/// </summary>
		public VerificationStatus Status { get; }

		/// <summary>
		/// 発生場所
		/// </summary>
		public string Source { get; }

		/// <summary>
		/// メッセージ
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="status">検証結果</param>
		/// <param name="source">発生場所</param>
		/// <param name="message">メッセージ</param>
		public VerifiedEventArgs(VerificationStatus status, string source, string message)
		{
			Status = status;
			Source = source;
			Message = message;
		}
	}
}
