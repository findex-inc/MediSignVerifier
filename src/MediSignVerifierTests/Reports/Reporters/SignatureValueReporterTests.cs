using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SignatureVerifier.Data;
using SignatureVerifier.Verifiers;

namespace SignatureVerifier.Reports.Reporters
{
	internal class SignatureValueReporterTests
	{
		[Test(Description = "ValidationDataへの変換エラー")]
		public void Generate_Error()
		{
			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "署名データ", "", "", "不明なエラー"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureValueReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("署名データ：");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(1));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "")?.Message, Is.EqualTo("不明なエラー"));
			});
		}

		[Test(Description = "単一署名")]
		public void Generate_SingleSignature()
		{
			var sigValue = new SignatureValueValidationData("SignatureValue"
				, "http://www.w3.org/2001/10/xml-exc-c14n#"
				, "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
				, "q4SdSA9xnhbunEvRz0Gx2w0Rkylt+apKs4nBVUYCqIw6gwjxKhJ/3R8o4zwaLE3lfl9dxDNnVWs9 f8m5AP23SbZ4mTIW6Fa4MsYnB1XHUzt4Lp889Ea38Zv84KGBKNRx4vKhT57KWY5uP5cHskZFHozM kg17YgUc0bC7hhIQzogDgjzq7R1PAq8UWQALjN98oryyTh0LMfDr76nNZqXiWMllXbPYAb7jmnKr 5zMTDlSOB00mUje+YcLL4VnASyfORjUH3ocAyUmf3dCXfA5q1g6HzaikNXEg+DmkmvFLafDv3HRr GSdI0ml6uhoMJJLz0f6J8HYlo8h+lZsPNZ8qQA==".ToBytes()
				, null);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.SignatureValueValidationData).Returns(sigValue);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名データ", "SignatureValue要素", "SignatureValue", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Prescription, "署名データ", "SignatureMethod要素", "SignatureValue", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureValueReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(処方)署名データ");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Id, Is.EqualTo("SignatureValue"));
				Assert.That(actual.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(actual.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "複数署名")]
		public void Generate_MultiSignature()
		{
			var sigValue1 = new SignatureValueValidationData("PrescriptionSignatureValue"
				, "http://www.w3.org/2001/10/xml-exc-c14n#"
				, "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
				, "q4SdSA9xnhbunEvRz0Gx2w0Rkylt+apKs4nBVUYCqIw6gwjxKhJ/3R8o4zwaLE3lfl9dxDNnVWs9 f8m5AP23SbZ4mTIW6Fa4MsYnB1XHUzt4Lp889Ea38Zv84KGBKNRx4vKhT57KWY5uP5cHskZFHozM kg17YgUc0bC7hhIQzogDgjzq7R1PAq8UWQALjN98oryyTh0LMfDr76nNZqXiWMllXbPYAb7jmnKr 5zMTDlSOB00mUje+YcLL4VnASyfORjUH3ocAyUmf3dCXfA5q1g6HzaikNXEg+DmkmvFLafDv3HRr GSdI0ml6uhoMJJLz0f6J8HYlo8h+lZsPNZ8qQA==".ToBytes()
				, null);

			var dispPrescriptionSign = new Mock<ISignature>();
			dispPrescriptionSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.DispPrescription);
			dispPrescriptionSign.SetupGet(x => x.SignatureValueValidationData).Returns(sigValue1);


			var sigValue2 = new SignatureValueValidationData("DispengingSignatureValue"
				, "http://www.w3.org/2001/10/xml-exc-c14n#"
				, "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
				, "kxVqcUn80ZU4dzsCOFDufRDMdN558yrdbLEpmpq6yB6uL74qh8CINDNe912qq+eOW/oCPA58FxT8 gnZehlUqOMq0B4q+HVSwan6UU8IE2jb5tXa/66OWldBsgJ5zOBaplLngKG+qY3tFpBbh7O/uIzgU oReWve1MKFpZMrwSrgbniyc8J5j/pTi1sYYOWxZk1374lVhHQfB4/+btJKsvuk30E5/rfrNf4++2 QJ7Sq0TEKlqrPljECghmZtzKB7ZQFS8WGqGBUuy6vKBCjmwNbUCj/TiSmRcu7ndL7guUjfLzR3fc c2y+lwEScfTmuHRhNsinYOiE0FjNUryJ2RD4Dw==".ToBytes()
				, null);

			var dispensingSign = new Mock<ISignature>();
			dispensingSign.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Dispensing);
			dispensingSign.SetupGet(x => x.SignatureValueValidationData).Returns(sigValue2);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "署名データ", "SignatureValue要素", "SignatureValue", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.DispPrescription, "署名データ", "SignatureMethod要素", "SignatureValue", null),

				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "署名データ", "SignatureValue要素", "SignatureValue", null),
				new VerificationResultItem(VerificationStatus.VALID, SignatureSourceType.Dispensing, "署名データ", "SignatureMethod要素", "SignatureValue", null),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureValueReporter(config);

			//Act1(DispPrescription)
			var actual = target.Generate(dispPrescriptionSign.Object, result);

			TestContext.WriteLine("(調剤済み処方)署名データ");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert1
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Id, Is.EqualTo("PrescriptionSignatureValue"));
				Assert.That(actual.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(actual.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});

			//Act2(Dispensing)
			actual = target.Generate(dispensingSign.Object, result);

			TestContext.WriteLine("(調剤済み処方)署名データ");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert2
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.Id, Is.EqualTo("DispengingSignatureValue"));
				Assert.That(actual.C14nAlgorithm, Is.EqualTo("EXC-C14N"));
				Assert.That(actual.SignatureAlgorithm, Is.EqualTo("SHA-256withRSA"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.VALID));
			});
		}

		[Test(Description = "検証エラーあり")]
		public void Generate_SingleSignatureError()
		{
			var sigValue = new SignatureValueValidationData("SignatureValue"
				, "http://www.w3.org/2001/10/xml-exc2-c14n#"
				, "http://www.w3.org/2001/04/xmldsig-more#rsa-sha999"
				, null
				, null);

			var signature = new Mock<ISignature>();
			signature.SetupGet(x => x.SourceType).Returns(SignatureSourceType.Prescription);
			signature.SetupGet(x => x.SignatureValueValidationData).Returns(sigValue);

			var resultItems = new List<VerificationResultItem>
			{
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "署名データ", "SignatureValue要素", "SignatureValue", "署名値の復号に失敗しました。"),
				new VerificationResultItem(VerificationStatus.INVALID, SignatureSourceType.Prescription, "署名データ", "SignatureMethod要素", "SignatureValue", "サポートされていない署名アルゴリズムが指定されています。"),
			};

			var result = new VerificationResult(resultItems.Select(m => m.Status).ToConclusion(), resultItems);

			var config = new ReportConfig();
			var target = new SignatureValueReporter(config);
			var actual = target.Generate(signature.Object, result);

			TestContext.WriteLine("(処方)署名データ");
			TestContext.WriteLine(TestData.ToJson(actual));

			//Assert
			Assert.Multiple(() =>
			{
				Assert.That(actual.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.Id, Is.EqualTo("SignatureValue"));
				Assert.That(actual.C14nAlgorithm, Is.EqualTo("http://www.w3.org/2001/10/xml-exc2-c14n#"));
				Assert.That(actual.SignatureAlgorithm, Is.EqualTo("http://www.w3.org/2001/04/xmldsig-more#rsa-sha999"));

				Assert.That(actual.ResultItems.Count, Is.EqualTo(2));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureValue要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
				Assert.That(actual.ResultItems.FirstOrDefault(m => m.ItemName == "SignatureMethod要素")?.Status, Is.EqualTo(VerificationStatus.INVALID));
			});
		}
	}
}
