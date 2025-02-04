using System.Collections.Generic;

namespace SignatureVerifier
{
	/// <summary>
	/// 検証データインターフェース
	/// </summary>
	public interface ISignedDocument
	{
		/// <summary>
		/// 処方箋種別（処方 or 調剤）
		/// </summary>
		DocumentType DocumentType { get; }

		/// <summary>
		/// ファイルパス
		/// </summary>
		string FilePath { get; }

		/// <summary>
		/// 実体（XmlDocument　or ...）
		/// </summary>
		object Raw { get; }

		/// <summary>
		/// 署名（Signature）リスト
		/// </summary>
		IEnumerable<ISignature> Signatures { get; }

		/// <summary>
		/// データ形式に応じた構造検証用検証器を作成
		/// </summary>
		/// <param name="config">検証オプション</param>
		/// <returns>構造検証器</returns>
		IStructureVerifier CreateStructureVerifier(VerificationConfig config);

	}
}
