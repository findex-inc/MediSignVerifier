using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.X509.Store;

namespace SignatureVerifier.Data.BouncyCastle.Pkix
{

	/// <summary>
	/// Provides internal utility methods for use with X.509 CRL and OCSP response validation..
	/// </summary>
	/// <remarks>
	/// Because I hesitated to use Emit, Cecil, etc. so I will refrection some of them using the code as a reference.<br />
	/// implementations using the following as a reference.<br />
	/// <see href="https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/Rfc3280CertPathUtilities.cs"/><br />
	/// Comments are unchanged.
	/// </remarks>
	/// <seealso href="https://www.bouncycastle.org/licence.html"/>
	internal static class PkixCertPathUtilities
	{

		public static IEnumerable<X509Crl> FindCrls(
			X509Certificate cert,
			PkixParameters paramsPKIX
			)
		{
			// from Rfc3280CertPathUtilities.CheckCrls()

			var duplicates = new HashSet<X509Crl>();

			CrlDistPoint crldp;

			try {
				crldp = CrlDistPoint.GetInstance(PkixCertPathValidatorUtilities
					.GetExtensionValue(cert, X509Extensions.CrlDistributionPoints));
			}
			catch (Exception e) {
				throw new ApplicationException("CRL distribution point extension could not be read.", e);
			}

			try {
				PkixCertPathValidatorUtilities.AddAdditionalStoresFromCrlDistributionPoint(crldp, paramsPKIX);
			}
			catch (Exception e) {
				throw new ApplicationException(
					"No additional CRL locations could be decoded from CRL distribution point extension.", e);
			}

			if (crldp != null) {
				DistributionPoint[] dps;
				try {
					dps = crldp.GetDistributionPoints();
				}
				catch (Exception e) {
					throw new ApplicationException("Distribution points could not be read.", e);
				}

				if (dps != null) {
					//unused status and reason
					//for (int i = 0; i < dps.Length && certStatus.Status == CertStatus.Unrevoked && !reasonsMask.IsAllReasons; i++)
					for (int i = 0; i < dps.Length; i++) {
						PkixParameters paramsPKIXClone = (PkixParameters)paramsPKIX.Clone();
						{
							var crls = FindCrl(dps[i], paramsPKIXClone, cert);

							foreach (var crl in crls) {
								if (!duplicates.Contains(crl)) {
									duplicates.Add(crl);
									yield return crl;
								}
							}
						}
					}
				}
			}

			/*
			 * If the revocation status has not been determined, repeat the process
			 * above with any available CRLs not specified in a distribution point
			 * but issued by the certificate issuer.
			 */

			//unused status and reason
			//if (certStatus.Status == CertStatus.Unrevoked && !reasonsMask.IsAllReasons)
			{
				{
					/*
					 * assume a DP with both the reasons and the cRLIssuer fields
					 * omitted and a distribution point name of the certificate
					 * issuer.
					 */

					X509Name issuer;
					try {
						issuer = X509Name.GetInstance(cert.IssuerDN.GetEncoded());
					}
					catch (Exception e) {
						throw new ApplicationException("Issuer from certificate for CRL could not be reencoded.", e);
					}
					DistributionPoint dp = new DistributionPoint(new DistributionPointName(0, new GeneralNames(
						new GeneralName(GeneralName.DirectoryName, issuer))), null, null);
					PkixParameters paramsPKIXClone = (PkixParameters)paramsPKIX.Clone();

					var crls = FindCrl(dp, paramsPKIXClone, cert);

					foreach (var crl in crls) {
						if (!duplicates.Contains(crl)) {
							duplicates.Add(crl);
							yield return crl;
						}
					}
				}
			}

		}

		private static IEnumerable<X509Crl> FindCrl(
			DistributionPoint dp,
			PkixParameters paramsPKIX,
			X509Certificate cert)
		{
			// from Rfc3280CertPathUtilities.CheckCrl()

			DateTime unused = DateTime.UtcNow;

			ISet crls = PkixCertPathValidatorUtilities.GetCompleteCrls(dp, cert, unused, paramsPKIX);

			foreach (X509Crl crl in crls) {
				yield return crl;
			}
		}



		private class PkixCertPathValidatorUtilities
		{
			public static Asn1Object GetExtensionValue(
				IX509Extension ext,
				DerObjectIdentifier oid)
			{
				Asn1OctetString bytes = ext.GetExtensionValue(oid);

				if (bytes == null) {

					return null;
				}

				return X509ExtensionUtilities.FromExtensionValue(bytes);
			}


			// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/PkixCertPathValidatorUtilities.cs#L1043
			private static readonly MethodInfo _reflectedAddAdditionalStoresFromCrlDistributionPoint = typeof(Org.BouncyCastle.Pkix.PkixCertPathValidatorUtilities)
					.GetMethod(nameof(AddAdditionalStoresFromCrlDistributionPoint), BindingFlags.NonPublic | BindingFlags.Static);

			public static void AddAdditionalStoresFromCrlDistributionPoint(
				CrlDistPoint crldp,
				PkixParameters pkixParams)
			{
				try {
					_reflectedAddAdditionalStoresFromCrlDistributionPoint.Invoke(null, new object[] { crldp, pkixParams });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}

				return;
			}


			private static readonly PkixCrlUtilities CrlUtilities = new PkixCrlUtilities();

			public static ISet GetCompleteCrls(
				DistributionPoint dp,
				object cert,
				DateTime currentDate,
				PkixParameters paramsPKIX)
			{
				X509CrlStoreSelector crlselect = new X509CrlStoreSelector();
				try {
					ISet issuers = new HashSet();
					if (cert is X509V2AttributeCertificate v2AttrCert) {
						issuers.Add(v2AttrCert
							.Issuer.GetPrincipals()[0]);
					}
					else {
						issuers.Add(GetIssuerPrincipal(cert));
					}
					PkixCertPathValidatorUtilities.GetCrlIssuersFromDistributionPoint(dp, issuers, crlselect, paramsPKIX);
				}
				catch (Exception e) {
					throw new Exception("Could not get issuer information from distribution point.", e);
				}

				if (cert is X509Certificate certificate) {
					crlselect.CertificateChecking = certificate;
				}
				else if (cert is X509V2AttributeCertificate) {
					crlselect.AttrCertChecking = (IX509AttributeCertificate)cert;
				}

				crlselect.CompleteCrlEnabled = true;
				ISet crls = CrlUtilities.FindCrls(crlselect, paramsPKIX, currentDate);

				// unchecked.

				//if (crls.IsEmpty) {
				//	if (cert is IX509AttributeCertificate) {
				//		IX509AttributeCertificate aCert = (IX509AttributeCertificate)cert;

				//		throw new Exception("No CRLs found for issuer \"" + aCert.Issuer.GetPrincipals()[0] + "\"");
				//	}
				//	else {
				//		X509Certificate xCert = (X509Certificate)cert;

				//		throw new Exception("No CRLs found for issuer \"" + xCert.IssuerDN + "\"");
				//	}
				//}

				return crls;
			}

			private static X509Name GetIssuerPrincipal(
				object cert)
			{
				if (cert is X509Certificate certificate) {
					return certificate.IssuerDN;
				}
				else {
					return ((IX509AttributeCertificate)cert).Issuer.GetPrincipals()[0];
				}
			}

			// https://github.com/bcgit/bc-csharp/blob/release/v1.9/crypto/src/pkix/PkixCertPathValidatorUtilities.cs#L753
			private static readonly MethodInfo _reflectedGetCrlIssuersFromDistributionPoint = typeof(Org.BouncyCastle.Pkix.PkixCertPathValidatorUtilities)
					.GetMethod(nameof(GetCrlIssuersFromDistributionPoint), BindingFlags.NonPublic | BindingFlags.Static);

			private static ISet GetCrlIssuersFromDistributionPoint(
				DistributionPoint dp,
				ICollection issuerPrincipals,
				X509CrlStoreSelector selector,
				PkixParameters pkixParams)
			{
				try {
					return (ISet)_reflectedGetCrlIssuersFromDistributionPoint.Invoke(null, new object[] { dp, issuerPrincipals, selector, pkixParams });
				}
				catch (TargetInvocationException e) {
					var inner = e.InnerException ?? e;
					throw new ApplicationException(inner.Message, inner);
				}
			}

		}
	}
}
