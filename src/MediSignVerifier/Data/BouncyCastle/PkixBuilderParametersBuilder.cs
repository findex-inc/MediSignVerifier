using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace SignatureVerifier.Data.BouncyCastle
{
	internal class PkixBuilderParametersBuilder
	{
		private readonly IEnumerable<X509Certificate> _trustes;
		private readonly X509CertStoreSelector _selector;

		private DateTime? _validDate;
		private PkixCertPathChecker _checker;
		private readonly IList<X509Certificate> _certs = new List<X509Certificate>();
		private readonly IList<X509Crl> _crls = new List<X509Crl>();

		public PkixBuilderParametersBuilder(IEnumerable<X509Certificate> trustes, X509CertStoreSelector selector)
		{
			_trustes = trustes;
			_selector = selector;
		}

		public PkixBuilderParameters Build()
		{
			IX509Store x509CertStore = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(_certs.ToArray()));

			IX509Store x509CrlStore = X509StoreFactory.Create(
				"CRL/Collection",
				new X509CollectionStoreParameters(_crls.ToArray()));

			var trust = new HashSet(_trustes.Select(x => new TrustAnchor(x, null)));

			var parameters = new PkixBuilderParameters(trust, _selector);

			parameters.AddStore(x509CertStore);
			parameters.AddStore(x509CrlStore);

			if (_validDate != null) {
				parameters.Date = new DateTimeObject(_validDate.Value);
			}

			if (_checker != null) {
				parameters.AddCertPathChecker(_checker);
			}

			return parameters;
		}

		public PkixBuilderParametersBuilder SetDate(DateTime validDate)
		{
			_validDate = validDate;
			return this;
		}
		internal PkixBuilderParametersBuilder AddPathChecker(PkixCertPathChecker checker)
		{
			_checker = checker;
			return this;
		}

		public PkixBuilderParametersBuilder AddStore(IEnumerable<X509Certificate> certs)
		{
			foreach (var cert in certs) {

				if (!_certs.Contains(cert)) {

					_certs.Add(cert);
				}
			}
			return this;
		}

		public PkixBuilderParametersBuilder AddStore(IEnumerable<X509Crl> crls)
		{
			foreach (var crl in crls) {

				if (!_crls.Contains(crl)) {

					_crls.Add(crl);
				}
			}
			return this;
		}

	}
}
