using System;
using Org.BouncyCastle.Asn1;

namespace SignatureVerifier.Data.BouncyCastle.Asn1.HPKI
{
	internal class CodedData
		: Asn1Encodable
	{
		public static CodedData GetInstance(object obj)
		{
			if (obj is CodedData data)
				return data;

			if (obj == null)
				return null;

			return new CodedData(Asn1Set.GetInstance(obj));
		}

		private CodedData(Asn1Set set)
		{
			if (set.Count < 1 || set.Count > 3) {
				throw new ArgumentException("Bad sequence size: " + set.Count, "set");
			}

			foreach (object obj in set) {
				if (obj is Asn1TaggedObject tagged) {
					switch (tagged.TagNo) {
						case 0:
							CodingSchemeReference = DerObjectIdentifier.GetInstance(tagged.GetObject());
							break;

						case 1:
							CodeDataValue = DerNumericString.GetInstance(tagged.GetObject());
							break;

						case 2:
							CodeDataFreeText = DerUtf8String.GetInstance(tagged.GetObject());
							break;

						default:
							throw new ArgumentException(String.Format("Bad tag [{0}]", tagged.TagNo), "set");
					}
				}
			}
		}

		public CodedData(DerObjectIdentifier reference, DerNumericString value, DerUtf8String freeText)
		{
			CodingSchemeReference = reference;
			CodeDataValue = value;
			CodeDataFreeText = freeText;
		}


		public DerObjectIdentifier CodingSchemeReference { get; private set; }

		public DerNumericString CodeDataValue { get; private set; }

		public DerUtf8String CodeDataFreeText { get; private set; }

		/**
         * https://www.mhlw.go.jp/stf/shingi/2r9852000002on3n-att/2r9852000002onaf.pdf
         * <pre>
         *
         *     CodedData :: = SET {
         *        codingSchemeReference         [0] OBJECT IDENTIFIER,
         *        -- Contains the ISO coding scheme Reference
         *        -- or local coding scheme reference achieving ISO or national registration.
         *        -- Local coding scheme reference in Japanese HPKI is id-jhpki-cdata (defined above)
         *        -- In this profile, use this OID: Note 2
         *        -- At least ONE of the following SHALL be present
         *        codeDataValue                 [1] NumericString OPTIONAL, -- Note 3 (Do not use)
         *        codeDataFreeText              [2] DirectoryString } -- Note 4
         *
         * </pre>
         */
		public override Asn1Object ToAsn1Object()
		{
			var v = new Asn1EncodableVector(new DerTaggedObject(true, 0, CodingSchemeReference));

			if (CodeDataValue != null) {
				v.AddOptional(new DerTaggedObject(true, 1, CodeDataValue));
			}

			v.Add(new DerTaggedObject(true, 2, CodeDataFreeText));

			return new DerSet(v);
		}

	}
}
