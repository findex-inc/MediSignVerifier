using System;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;

namespace SignatureVerifier.Data.BouncyCastle.Asn1.HPKI
{
	public class HCActorTests
	{
		[Test]
		[TestCase(@"MCSgIjEgoAwGCiqDCIaRDwEGAQGiEAwOTWVkaWNhbCBEb2N0b3I=")]
		[TestCase(@"MCegJTEjoAwGCiqDCIaRDwEGAQGhBRIDMTIzogwMClBoYXJtYWNpc3Q=")]
		[TestCase(@"MCugJTEjoAwGCiqDCIaRDwEGAQGhBRIDMTIzogwMClBoYXJtYWNpc3ShAjAA")]
		public void WhenCallingToAsn1Object_thenReturnToOriginal(string base64)
		{
			byte[] der = Convert.FromBase64String(base64);
			var sequence = Asn1Sequence.GetInstance(der);

			var hcActor = HCActor.GetInstance(sequence);

			var actual = hcActor.ToAsn1Object();
			var actualBase64 = Convert.ToBase64String(actual.GetDerEncoded());

			Assert.That(actualBase64, Is.EqualTo(base64));

			return;
		}

	}
}
