using System.IO;
using Org.BouncyCastle.X509;

namespace SignatureVerifier.Data
{
	/// <summary>
	/// X.509 v3 証明書パーサー
	/// </summary>
	public sealed class CertificateDataParser
	{
		private readonly X509CertificateParser _internalParser = new X509CertificateParser();

		/// <summary>
		/// コンストラクター
		/// </summary>
		/// <param name="source">取得元をあらわす値</param>
		public CertificateDataParser(string source)
		{
			this.SourceName = source;
		}

		/// <summary>
		/// 取得元をあらわす値を取得します。
		/// </summary>
		private string SourceName { get; }

		/// <summary>
		/// ファイルパスを指定して X.509 v3 証明書を読み込み <see cref="CertificateData"/>データを作成します。
		/// </summary>
		/// <remarks>
		/// <para>DER、PEM どちらのエンコード形式も読み込むことができます。</para>
		/// </remarks>
		/// <param name="path">証明書ファイルのパス</param>
		/// <returns><see cref="CertificateData"/>データ</returns>
		public CertificateData ReadCertificateData(string path)
			=> ReadCertificateData(File.ReadAllBytes(path).RemoveUTF8BOM());

		/// <summary>
		/// バイナリ配列を指定して X.509 v3 証明書を読み込み <see cref="CertificateData"/>データを作成します。
		/// </summary>
		/// <param name="bytes">バイナリ配列</param>
		/// <returns><see cref="CertificateData"/>データ</returns>
		public CertificateData ReadCertificateData(byte[] bytes)
			=> new CertificateData(SourceName,
				_internalParser.ReadCertificate(bytes).GetEncoded());

	}
}
