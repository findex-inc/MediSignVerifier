using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Verifiers.BouncyCastle.Pkix
{
	/// <summary>
	/// Implementation of X.509 CRL and OCSP response validation according to RFC 3280 (not RFC 5280?).
	/// </summary>
	/// <remarks>
	/// implementations using the following as a reference.<br />
	/// <see href="https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/PkixCertPathValidator.cs"/><br />
	/// Comments are unchanged.
	/// </remarks>
	/// <seealso href="https://www.bouncycastle.org/licence.html"/>
	internal class RevocationPkixCertPathValidator
	{
		public virtual PkixCertPathValidatorResult Validate(
			PkixCertPath certPath,
			IEnumerable<OcspResp> ocsps,
			PkixParameters paramsPKIX)
		{
			// !Notice!
			// Skips checking all parameters.

			//
			// 6.1.1 - inputs
			//

			//
			// (a)
			//
			IList certs = certPath.Certificates;
			int n = certs.Count;

			//
			// (d)
			//
			TrustAnchor trust;
			try {
				trust = PkixCertPathValidatorUtilities.FindTrustAnchor(
					(X509Certificate)certs[certs.Count - 1],
					paramsPKIX.GetTrustAnchors());
			}
			catch (Exception e) {
				throw new PkixCertPathValidatorException(e.Message, e.InnerException, certPath, certs.Count - 1);
			}

			//
			// 6.1.2 - setup
			//

			//
			// (g), (h), (i), (j)
			//
			AsymmetricKeyParameter workingPublicKey;
			X509Name workingIssuerName;

			X509Certificate sign = trust.TrustedCert;
			try {
				if (sign != null) {
					workingIssuerName = sign.SubjectDN;
					workingPublicKey = sign.GetPublicKey();
				}
				else {
					workingIssuerName = new X509Name(trust.CAName);
					workingPublicKey = trust.CAPublicKey;
				}
			}
			catch (ArgumentException ex) {
				throw new PkixCertPathValidatorException("Subject of trust anchor could not be (re)encoded.", ex, certPath,
						-1);
			}

			for (var index = certs.Count - 1; index >= 0; index--) {

				var i = n - index;
				var cert = (X509Certificate)certs[index];

				//
				// 6.1.3
				//

				//Rfc3280CertPathUtilities.ProcessCertA(certPath, paramsPkix, index, workingPublicKey,
				//	workingIssuerName, sign);
				{
					//
					// (a) (3)
					//
					if (paramsPKIX.IsRevocationEnabled) {

						// --- CUSTOM POINT ---
						var foundOcsp = ocsps?.FindByCertificate(cert, sign);
						if (foundOcsp != null) {
							try {
								foundOcsp.Validate(
									cert,
									PkixCertPathValidatorUtilities.GetValidCertDateFromValidityModel(paramsPKIX, certPath, index),
									sign);
							}
							catch (ApplicationException e) {
								var cause = e.InnerException ?? e;
								throw new PkixCertPathValidatorException(e.Message, cause, certPath, index);
							}

							// 6.3.3 (f)Obtain and validate the certification path for the issuer of the complete CRL.
							//
							// The trust anchor for the certification path
							// MUST be the same as the trust anchor used to validate the target certificate.

							var signedCert = foundOcsp.GetSignedCert();
							if (!signedCert.Equals(sign)) {

								var builder = new PkixCertPathBuilder();

								var temp = (PkixParameters)paramsPKIX.Clone();
								temp.SetTargetCertConstraints(new X509CertStoreSelector
								{
									Certificate = signedCert
								});

								var parameters = PkixBuilderParameters.GetInstance(temp);
								parameters.IsRevocationEnabled = false;

								// TrustAncherまでたどりつかない場合には例外が発生する。
								_ = builder.Build(parameters);
							}

							continue;
						}

						try {
							CheckCrls(
								paramsPKIX,
								cert,
								PkixCertPathValidatorUtilities.GetValidCertDateFromValidityModel(paramsPKIX, certPath, index),
								sign,
								workingPublicKey,
								certs);
						}
						catch (Exception e) {
							var cause = e.InnerException ?? e;
							throw new PkixCertPathValidatorException(e.Message, cause, certPath, index);
						}
					}
				}

				//
				// 6.1.4
				//

				if (i != n) {

					if (cert != null && cert.Version == 1) {
						// we've found the trust anchor at the top of the path, ignore and keep going
						if ((i == 1) && cert.Equals(trust.TrustedCert))
							continue;

						throw new PkixCertPathValidatorException(
							"Version 1 certificates can't be used as CA ones.", null, certPath, index);
					}

					// set signing certificate for next round
					sign = cert;

					// (c)
					// workingIssuerName = sign.SubjectDN;

					// (d)
					try {
						workingPublicKey = PkixCertPathValidatorUtilities.GetNextWorkingKey(certPath.Certificates, index);
					}
					catch (PkixCertPathValidatorException e) {
						throw new PkixCertPathValidatorException("Next working key could not be retrieved.", e, certPath, index);
					}

				}
			}

			return null;
		}

		private static void CheckCrls(
			PkixParameters paramsPKIX,
			X509Certificate cert,
			DateTime validDate,
			X509Certificate sign,
			AsymmetricKeyParameter workingPublicKey,
			IList certPathCerts)
		{
			Rfc3280CertPathUtilities.CheckCrls(
				paramsPKIX,
				cert,
				validDate,
				sign,
				workingPublicKey,
				certPathCerts);
		}

		private static class Rfc3280CertPathUtilities
		{
			private static readonly MethodInfo _reflectedCheckCrls = typeof(Org.BouncyCastle.Pkix.Rfc3280CertPathUtilities)
					.GetMethod(nameof(CheckCrls), BindingFlags.NonPublic | BindingFlags.Static);

			public static void CheckCrls(
				PkixParameters paramsPKIX,
				X509Certificate cert,
				DateTime validDate,
				X509Certificate sign,
				AsymmetricKeyParameter workingPublicKey,
				IList certPathCerts)
			{
				try {
					_reflectedCheckCrls.Invoke(null, new object[] { paramsPKIX, cert, validDate, sign, workingPublicKey, certPathCerts });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}

				return;
			}
		}

		private class PkixCertPathValidatorUtilities
		{
			private static readonly MethodInfo _reflectedFindTrustAnchor = typeof(Org.BouncyCastle.Pkix.PkixCertPathValidatorUtilities)
					.GetMethod(nameof(FindTrustAnchor), BindingFlags.NonPublic | BindingFlags.Static);

			public static TrustAnchor FindTrustAnchor(X509Certificate x509Certificate, ISet set)
			{
				try {
					return (TrustAnchor)_reflectedFindTrustAnchor.Invoke(null, new object[] { x509Certificate, set });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}
			}

			private static readonly MethodInfo _reflectedGetValidCertDateFromValidityModel = typeof(Org.BouncyCastle.Pkix.PkixCertPathValidatorUtilities)
				.GetMethod(nameof(GetValidCertDateFromValidityModel), BindingFlags.NonPublic | BindingFlags.Static);

			public static DateTime GetValidCertDateFromValidityModel(
				PkixParameters paramsPkix,
				PkixCertPath certPath,
				int index)
			{
				try {
					return (DateTime)_reflectedGetValidCertDateFromValidityModel.Invoke(null, new object[] { paramsPkix, certPath, index });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}
			}

			private static readonly MethodInfo _reflectedGetNextWorkingKey = typeof(Org.BouncyCastle.Pkix.PkixCertPathValidatorUtilities)
				.GetMethod(nameof(GetNextWorkingKey), BindingFlags.NonPublic | BindingFlags.Static);

			public static AsymmetricKeyParameter GetNextWorkingKey(
				IList certs,
				int index)
			{
				try {
					return (AsymmetricKeyParameter)_reflectedGetNextWorkingKey.Invoke(null, new object[] { certs, index });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}
			}

		}

	}
}
