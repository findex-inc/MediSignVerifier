using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignatureVerifier.Reports
{
	/// <summary>
	/// 証明書の失効理由を示します。
	/// </summary>
	[JsonConverter(typeof(Converter))]
	public sealed class CertificateRevocationReason
	{
		/// <summary>
		/// 不明。
		/// </summary>
		public static readonly CertificateRevocationReason Unknown = new CertificateRevocationReason(-1, "unknown");

		/// <summary>
		/// <c>unspecified (0)</c>: その他の理由。
		/// </summary>
		public static readonly CertificateRevocationReason Unspecified = new CertificateRevocationReason(0, "unspecified");

		/// <summary>
		/// <c>keyCompromise (1)</c>: 利用者の秘密鍵が危殆化した。
		/// </summary>
		public static readonly CertificateRevocationReason KeyCompromise = new CertificateRevocationReason(1, "keyCompromise");

		/// <summary>
		/// <c>cACompromise (2)</c>: CAの秘密鍵が危殆化した。
		/// </summary>
		public static readonly CertificateRevocationReason CACompromise = new CertificateRevocationReason(2, "cACompromise");

		/// <summary>
		/// <c>affiliationChanged (3)</c>: 証明書情報の記載内容に変更が生じた。
		/// </summary>
		public static readonly CertificateRevocationReason AffiliationChanged = new CertificateRevocationReason(3, "affiliationChanged");

		/// <summary>
		/// <c>superseded (4)</c>: 新しい証明書が発行された。
		/// </summary>
		public static readonly CertificateRevocationReason Superseded = new CertificateRevocationReason(4, "superseded");

		/// <summary>
		/// <c>cessationOfOperation (5)</c>: 証明書を使用しなくなった。
		/// </summary>
		public static readonly CertificateRevocationReason CessationOfOperation = new CertificateRevocationReason(5, "cessationOfOperation");

		/// <summary>
		/// <c>certificateHold (6)</c>: 秘密鍵の安全性に疑義が生じたため、証明書を一時的に保留した。
		/// </summary>
		public static readonly CertificateRevocationReason CertificateHold = new CertificateRevocationReason(6, "certificateHold");

		// value 7 is not used

		/// <summary>
		/// <c>removeFromCRL (8)</c>: 証明書が <c>certificateHold</c> になりましたが、現在は失効から削除された。
		/// </summary>
		public static readonly CertificateRevocationReason RemoveFromCrl = new CertificateRevocationReason(8, "removeFromCRL");

		/// <summary>
		/// <c>privilegeWithdrawn (9)</c>: 証明書を利用する権利をはく奪した。
		/// </summary>
		public static readonly CertificateRevocationReason PrivilegeWithdrawn = new CertificateRevocationReason(9, "privilegeWithdrawn");

		/// <summary>
		/// <c>aACompromise (10)</c>: AAの秘密鍵が危殆化した。
		/// </summary>
		public static readonly CertificateRevocationReason AACompromise = new CertificateRevocationReason(10, "aACompromise");

		private CertificateRevocationReason(int value, string name, string display = null)
		{
			Value = value;
			Name = name;
			Display = display ?? name;
		}

		/// <summary>
		/// RFC 5280 に記載された理由コードを取得します。
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// RFC 5280 に記載された理由コードに対する定義値名を取得します。
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 表示用の名称を取得します。
		/// </summary>
		public string Display { get; }

		///	<inheritdoc />
		public override string ToString()
		{
			var builder = new StringBuilder(nameof(CertificateRevocationReason));
			builder.Append(":{");
			builder.Append($"\"value\" : {Value}");
			builder.Append($", \"name\" : \"{Name}\"");
			builder.Append($", \"display\" : \"{Display}\" ");
			builder.Append("}");
			return builder.ToString();
		}


		private static readonly Lazy<IEnumerable<CertificateRevocationReason>> values = new Lazy<IEnumerable<CertificateRevocationReason>>(() =>
			typeof(CertificateRevocationReason).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Select(f => f.GetValue(null))
				.Cast<CertificateRevocationReason>());

		/// <summary>
		/// 理由コードから失効理由を取得します。
		/// </summary>
		/// <param name="value">理由コード</param>
		/// <returns>失効理由</returns>
		public static CertificateRevocationReason Parse(int value)
		{
			return values.Value.FirstOrDefault(x => x.Value == value) ?? Unknown;
		}


		internal class Converter : JsonConverter<CertificateRevocationReason>
		{
			public override CertificateRevocationReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}

			public override void Write(Utf8JsonWriter writer, CertificateRevocationReason value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.Display);
			}
		}

	}
}
