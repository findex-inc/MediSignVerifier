using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using SignatureVerifier.Data.BouncyCastle;

namespace SignatureVerifier.Security.Cryptography.Xml
{
	/// <summary>
	/// System.Security.Cryptographyアクセス用ヘルパークラス
	/// </summary>
	internal class SecurityCryptoXmlHelper
	{
		private static readonly Assembly Security_Assembly = GetInstance();

		private static Assembly GetInstance()
		{
			//#if NET35
			//return Assembly.Load("System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

#if NET45_OR_GREATER
			return Assembly.Load("System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

#elif NETSTANDARD2_1_OR_GREATER
			return Assembly.Load("System.Security.Cryptography.Xml, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");

#else
			return null;

#endif
		}

		private static readonly Type CanonicalXmlNodeList_Type = Security_Assembly.GetType("System.Security.Cryptography.Xml.CanonicalXmlNodeList");
		private static readonly Type Utils_Type = Security_Assembly.GetType("System.Security.Cryptography.Xml.Utils");

#if NETSTANDARD2_1_OR_GREATER
		private static readonly Type CryptoHelpers_Type = Security_Assembly.GetType("System.Security.Cryptography.Xml.CryptoHelpers");
#endif

		//Reference
		private static PropertyInfo _reference_SignedXml;

		internal static PropertyInfo Reference_SignedXml
		{
			get
			{
				if (_reference_SignedXml == null) {
					_reference_SignedXml = typeof(Reference).GetProperty("SignedXml", BindingFlags.NonPublic | BindingFlags.Instance);
				}

				return _reference_SignedXml;
			}
		}

		private static MethodInfo _reference_CalculateHashValue;

		internal static MethodInfo Reference_CalculateHashValue
		{
			get
			{
				if (_reference_CalculateHashValue == null) {
					_reference_CalculateHashValue = typeof(Reference).GetMethod("CalculateHashValue", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _reference_CalculateHashValue;
			}
		}

		//CryptoHelpers
		private static MethodInfo _cryptoHelpers_CreateFromName;

		internal static MethodInfo CryptoHelpers_CreateFromName
		{
			get
			{
				if (_cryptoHelpers_CreateFromName == null) {
#if NETSTANDARD2_1_OR_GREATER
					_cryptoHelpers_CreateFromName = CryptoHelpers_Type.GetMethod("CreateFromName", BindingFlags.Public | BindingFlags.Static);
#else
					_cryptoHelpers_CreateFromName = Utils_Type.GetMethod("CreateFromName", BindingFlags.Static | BindingFlags.NonPublic);
#endif
				}
				return _cryptoHelpers_CreateFromName;
			}
		}

		private static MethodInfo _cryptoHelpers_CreateHashAlgorithmFromName;

		internal static MethodInfo CryptoHelpers_CreateHashAlgorithmFromName
		{
			get
			{
				if (_cryptoHelpers_CreateHashAlgorithmFromName == null) {
					_cryptoHelpers_CreateHashAlgorithmFromName = CryptoHelpers_CreateFromName.MakeGenericMethod(new[] { typeof(HashAlgorithm) });
				}
				return _cryptoHelpers_CreateHashAlgorithmFromName;
			}
		}

		private static MethodInfo _cryptoHelpers_CreateSignatureDescriptionFromName;

		internal static MethodInfo CryptoHelpers_CreateSignatureDescriptionFromName
		{
			get
			{
				if (_cryptoHelpers_CreateSignatureDescriptionFromName == null) {
					_cryptoHelpers_CreateSignatureDescriptionFromName = CryptoHelpers_CreateFromName.MakeGenericMethod(new[] { typeof(SignatureDescription) });
				}
				return _cryptoHelpers_CreateSignatureDescriptionFromName;
			}
		}

		private static MethodInfo _cryptoHelpers_CreateTransformFromName;

		internal static MethodInfo CryptoHelpers_CreateTransformFromName
		{
			get
			{
				if (_cryptoHelpers_CreateTransformFromName == null) {
					_cryptoHelpers_CreateTransformFromName = CryptoHelpers_CreateFromName.MakeGenericMethod(new[] { typeof(Transform) });
				}
				return _cryptoHelpers_CreateTransformFromName;
			}
		}

		//Utils
		private static MethodInfo _utils_GetPropagatedAttributes;

		internal static MethodInfo Utils_GetPropagetedAttributes
		{
			get
			{
				if (_utils_GetPropagatedAttributes == null) {
					_utils_GetPropagatedAttributes = Utils_Type.GetMethod("GetPropagatedAttributes", BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _utils_GetPropagatedAttributes;
			}
		}

		private static MethodInfo _utils_PreProcessElementInput;

		internal static MethodInfo Utils_PreProcessElementInput
		{
			get
			{
				if (_utils_PreProcessElementInput == null) {
					_utils_PreProcessElementInput = Utils_Type.GetMethod("PreProcessElementInput", BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _utils_PreProcessElementInput;
			}
		}

		private static MethodInfo _utils_PreProcessDocumentInput;

		internal static MethodInfo Utils_PreProcessDocumentInput
		{
			get
			{
				if (_utils_PreProcessDocumentInput == null) {
					_utils_PreProcessDocumentInput = Utils_Type.GetMethod("PreProcessDocumentInput", BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _utils_PreProcessDocumentInput;
			}
		}

		private static MethodInfo _utils_DiscardComments;

		internal static MethodInfo Utils_DiscardComments
		{
			get
			{
				if (_utils_DiscardComments == null) {
					_utils_DiscardComments = Utils_Type.GetMethod("DiscardComments", BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _utils_DiscardComments;
			}
		}

		private static MethodInfo _utils_AddNameSpaces;

		internal static MethodInfo Utils_AddNameSpaces
		{
			get
			{
				if (_utils_AddNameSpaces == null) {
					_utils_AddNameSpaces = Utils_Type.GetMethod("AddNamespaces", BindingFlags.NonPublic | BindingFlags.Static, null,
						new Type[] { typeof(XmlElement), CanonicalXmlNodeList_Type }, null);
				}
				return _utils_AddNameSpaces;
			}
		}

		private static MethodInfo _utils_GetIdFromLocalUri;

		internal static MethodInfo Utils_GetIdFromLocalUri
		{
			get
			{
				if (_utils_GetIdFromLocalUri == null) {
					_utils_GetIdFromLocalUri = Utils_Type.GetMethod("GetIdFromLocalUri", BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _utils_GetIdFromLocalUri;
			}
		}

		private static ConstructorInfo _canonicalXmlNodeList_Constructor;

		internal static ConstructorInfo CanonicalXmlNodeList_Constructor
		{
			get
			{
				if (_canonicalXmlNodeList_Constructor == null) {
					_canonicalXmlNodeList_Constructor = CanonicalXmlNodeList_Type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
				}
				return _canonicalXmlNodeList_Constructor;
			}
		}

		//TransformChain
		private static MethodInfo _transformChain_TransformToOctetStream;
		internal static MethodInfo TransformChain_TransformToOctetStream
		{
			get
			{
				if (_transformChain_TransformToOctetStream == null) {
					_transformChain_TransformToOctetStream = typeof(TransformChain).GetMethod("TransformToOctetStream", BindingFlags.Instance | BindingFlags.NonPublic, null,
						new Type[] { typeof(XmlDocument), typeof(XmlResolver), typeof(string) }, null);

				}
				return _transformChain_TransformToOctetStream;
			}
		}

		private static MethodInfo _transformChain_LoadXml;
		internal static MethodInfo TransformChain_LoadXml
		{
			get
			{
				if (_transformChain_LoadXml == null) {
					_transformChain_LoadXml = typeof(TransformChain).GetMethod("LoadXml", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _transformChain_LoadXml;
			}
		}

		private static FieldInfo _xmlDsigXPathTransform_NamespaceManager;
		internal static FieldInfo XmlDsigXPathTransform_NamespaceManager
		{
			get
			{
				if (_xmlDsigXPathTransform_NamespaceManager == null) {
					_xmlDsigXPathTransform_NamespaceManager = typeof(XmlDsigXPathTransform).GetField("_nsm", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _xmlDsigXPathTransform_NamespaceManager;
			}
		}

		/// <summary>
		/// 名称からTransformを作成
		/// </summary>
		/// <param name="algorithm"></param>
		/// <returns></returns>
		internal static Transform CreateTransform(string algorithm)
		{
			return (Transform)CryptoHelpers_CreateTransformFromName.Invoke(null, new object[] { algorithm });
		}

		/// <summary>
		/// サポートされたハッシュアルゴリズムか
		/// </summary>
		/// <param name="algorithm"></param>
		/// <returns></returns>
		internal static bool IsSupportedHashAlgorithm(string algorithm)
		{
			return CreateHashAlgorithmFromName(algorithm) != null;
		}

		/// <summary>
		/// 名称からハッシュアルゴリズムを作成
		/// </summary>
		/// <param name="algorithm"></param>
		/// <returns></returns>
		internal static HashAlgorithm CreateHashAlgorithmFromName(string algorithm)
		{
			return (HashAlgorithm)CryptoHelpers_CreateHashAlgorithmFromName.Invoke(null, new object[] { algorithm });
		}

		/// <summary>
		/// サポートされた署名アルゴリズムか
		/// </summary>
		/// <param name="algorithm"></param>
		/// <returns></returns>
		internal static bool IsSupportedSignatureAlgorithm(string algorithm)
		{
			return algorithm.ToBCMechanism() != null;
		}
	}
}
