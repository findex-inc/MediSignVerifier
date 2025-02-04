namespace SignatureVerifier.Data
{
	/// <summary>
	/// 証明書の拡張領域に設定される hcRole（healthcare Role）を表します。
	/// </summary>
	/// <remarks>
	/// ISO17090（Health Informatics -Public Key Infrastructure）で定義された医療従事者の資格を表します。
	/// </remarks>
	public class HcRole
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="oid">hcRoleのOIDを指定します。</param>
		/// <param name="role">医療従事者の資格名を設定します。</param>
		public HcRole(string oid, string role)
		{
			Oid = oid;
			RoleName = role;
		}

		/// <summary>
		/// hcRoleのOIDを取得します。
		/// </summary>
		/// <remarks>
		/// <para>
		///  OID は、{ iso(1) member-body(2) jp(392) mhlw(100495) jhpki(1) hcRole(6) national-coding-scheme-reference(2) version(1) }を用います。
		/// </para>
		/// </remarks>
		public string Oid { get; }

		/// <summary>
		/// 医療従事者の資格名を取得します。
		/// </summary>
		/// <remarks>
		/// HPKI 資格名テーブルの一例：
		///   <list type="table">
		///   <listHeader>
		///     <term>資格名（国家資格）</term>
		///     <term>説明</term>
		///   </listHeader>
		///   <item>
		///     <description>‘Medical Doctor’</description>
		///     <description>医師</description>
		///   </item>
		///   <item>
		///     <description>‘Pharmacist’</description>
		///     <description>薬剤師</description>
		///   </item>
		///   </list>
		/// </remarks>
		public string RoleName { get; }

	}

}
