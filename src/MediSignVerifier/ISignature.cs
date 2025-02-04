using System;
using System.Collections.Generic;
using SignatureVerifier.Data;

namespace SignatureVerifier
{
	/// <summary>
	/// 署名部インターフェース
	/// </summary>
	public interface ISignature
	{
		/// <summary> AdESレベル </summary>
		ESLevel ESLevel { get; }

		/// <summary> 署名元データ種別 </summary>
		SignatureSourceType SourceType { get; }

		/// <summary> ノードパス </summary>
		string Path { get; }

		/// <summary> 署名者証明書検証情報 </summary>
		ISigningCertificateValidationData SigningCertificateValidationData { get; }

		/// <summary> 参照データ検証情報 </summary>
		IEnumerable<ReferenceValidationData> ReferenceValidationData { get; }

		/// <summary> 署名データ検証情報 </summary>
		SignatureValueValidationData SignatureValueValidationData { get; }

		/// <summary> 署名タイムスタンプ検証情報 </summary>
		TimeStampValidationData SignatureTimeStampValidationData { get; }

		/// <summary> アーカイブタイムスタンプ検証情報 </summary>
		IEnumerable<ArchiveTimeStampValidationData> ArchiveTimeStampValidationData { get; }

		/// <summary> 署名タイムスタンプ時刻 </summary>
		DateTime? SignatureTimeStampGenTime { get; }

		/// <summary> 最初のアーカイブタイムスタンプ時刻 </summary>
		DateTime? OldestArchiveTimeStampGenTime { get; }

	}
}
