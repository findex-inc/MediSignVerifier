using System;
using Org.BouncyCastle.Asn1;

namespace SignatureVerifier.Data.BouncyCastle.Asn1.HPKI
{
	internal class HCActor
		: Asn1Encodable
	{
		public static HCActor GetInstance(object obj)
		{
			if (obj is HCActor actor)
				return actor;

			if (obj == null)
				return null;

			return new HCActor(Asn1Sequence.GetInstance(obj));
		}


		private HCActor(Asn1Sequence seq)
		{
			if (seq.Count < 1 || seq.Count > 2) {
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
			}

			foreach (object obj in seq) {
				if (obj is Asn1TaggedObject tagged) {
					switch (tagged.TagNo) {
						case 0:
							CodedData = CodedData.GetInstance(tagged.GetObject());
							break;

						case 1:
							RegionalHCActorData = (Asn1Sequence)tagged.GetObject();
							break;

						default:
							throw new ArgumentException(string.Format("Bad tag [{0}]", tagged.TagNo), "seq");
					}
				}
			}
		}

		public HCActor(CodedData codedData, Asn1Sequence regionalHCActorData = null)
		{
			CodedData = codedData;
			RegionalHCActorData = regionalHCActorData;
		}

		public CodedData CodedData { get; private set; }

		public Asn1Sequence RegionalHCActorData { get; private set; }


		/**
         * https://www.mhlw.go.jp/stf/shingi/2r9852000002on3n-att/2r9852000002onaf.pdf
         * <pre>
         *
         *     HCActor :: = SEQUENCE {
         *        codedData	                   [0] CodedData,
         *        regionalHCActorData          [1] SEQUENCE OF RegionalData OPTIONAL } --Note1(Do not use)
         *
         * </pre>
         */
		public override Asn1Object ToAsn1Object()
		{
			var v = new Asn1EncodableVector(new DerTaggedObject(true, 0, CodedData.ToAsn1Object()));

			if (RegionalHCActorData != null) {
				v.AddOptional(new DerTaggedObject(true, 1, RegionalHCActorData));
			}

			return new DerSequence(v);
		}

	}

}
