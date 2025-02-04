
## MediSignVerifier

**MediSignVerifier** は、.NET 向けのオープンソースライブラリで、日本の電子処方箋に付与された電子署名を検証を目的としています。

**対応フレームワーク:**

* .NET Standard 2.1
* .NET Framework 4.6.2

**機能:**

* 電子処方箋データの電子署名を検証します。
* 署名時刻、署名者情報などの署名情報を取得します。

**今後の予定:**
* 電子処方箋以外の文書への対応を予定しています。

**プロジェクト説明:**
1. MediSignVerifier
ライブラリ本体、署名の検証を実施します。
1. EPDVerifyCmd
コマンドによる署名を実施し、結果をXMLファイルとしてレポートします。
内部的にはMediSignVerifierをCallしています。
1.MediSignVerifier.Tests
テスト用プロジェクトになります。


**使用方法:**
1. ソースコードに以下のようなコードを追加します。

```csharp
using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using SignatureVerifier;

public void Verify(Options options)
{
    _logger.Trace("Start.");

    var doc = SignedDocumentXml.Load(options.TargetPath);

    var vconfig = new VerificationConfig()
    {
        HPKIValidationEnabled = options.HPKIValidationEnabled!.Value,
    };
    var result = VerifyDocument(options, vconfig, doc);

    var exitcode = (int)result.Status;
    Environment.ExitCode = exitcode;
    _logger.Info($"検証結果: {result.Status}");

    _logger.Trace($"End");
    return;
}

private static SignatureVerificationResult VerifyDocument(Options options, VerificationConfig config, SignedDocumentXml doc)
{
    var verifier = new SignatureVerifier.SignatureVerifier(config);
    verifier.VerifiedEvent += OnVerifiedEvent;

    var result = verifier.Verify(doc, options.VerificationTime.Value);

    return result;

    static void OnVerifiedEvent(object sender, VerifiedEventArgs e)
    {
        _logger.Warn($"Accept event [{e.Status}]: -> {e.Message}");
    }
}

```

**免責事項:**

本ソフトウェアは、現状のまま提供され、明示または黙示を問わず、いかなる種類の保証もありません。
本ソフトウェアの使用に起因または関連して発生したいかなる損害（直接的、間接的、偶発的、特別、結果的、懲罰的なものを含むがこれらに限定されない）についても、作者は一切の責任を負いません。