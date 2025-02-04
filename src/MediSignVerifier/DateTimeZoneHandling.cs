using System;

namespace SignatureVerifier
{
	/// <summary>
	/// 文字列と <see cref="DateTime"/> の間で変換する際の時刻値の処理方法を指定します。
	/// </summary>
	public enum DateTimeZoneHandling
	{
		/// <summary>
		/// 現地時間として扱います。 <see cref="DateTime"/> オブジェクトが協定世界時 (UTC) を表す場合、現地時間に変換されます。
		/// </summary>
		Local = 0,

		/// <summary>
		/// UTC として扱います。 <see cref="DateTime"/> オブジェクトが現地時間を表す場合、UTC に変換されます。
		/// </summary>
		Utc = 1,

		/// <summary>
		/// 変換時にタイムゾーン情報を保持する必要があります。
		/// </summary>
		RoundtripKind = 3
	}

}
