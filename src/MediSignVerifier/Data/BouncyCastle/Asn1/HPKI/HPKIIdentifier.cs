using Org.BouncyCastle.Asn1;

namespace SignatureVerifier.Data.BouncyCastle.Asn1.HPKI
{
	internal sealed class HPKIIdentifier : DerObjectIdentifier
	{
		private HPKIIdentifier(string identifier) : base(identifier)
		{
		}

		public static readonly HPKIIdentifier id_hcpki = new HPKIIdentifier("1.0.17090");

		public static readonly HPKIIdentifier id_hcpki_at = new HPKIIdentifier(id_hcpki + ".0");
		public static readonly HPKIIdentifier id_hcpki_at_healthcareactor = new HPKIIdentifier(id_hcpki_at + ".1");
		public static readonly HPKIIdentifier id_hcpki_cd = new HPKIIdentifier(id_hcpki + ".1");

		public static readonly HPKIIdentifier id_jhpki = new HPKIIdentifier("1.2.392.100495.1");
		public static readonly HPKIIdentifier id_jhpki_cdata = new HPKIIdentifier(id_jhpki + ".6.1.1");

	}
}
