
# MediSignVerifierとは
**MediSignVerifier** は、Microsoft Visual Studio C#で作成された、.NET 向けのオープンソースライブラリです。
日本の電子処方箋に付与された電子署名の検証を目的としています。

## 機能
* 電子処方箋データの電子署名を検証します。
  (ES-BES, ES-T, ES-XL , EX-A)
* 署名時刻、署名者情報などの署名情報を取得します。
* XAdESのXMLスキーマの検証を行います。
* ライブラリ形式およびコマンド形式の２つを提供しています。

## Copyright
本プログラムは、Findex Inc.(https://findex.co.jp)の著作物です。

## 対応フレームワーク
* .NET Standard 2.1
* .NET Framework 4.6.2

## 取得先
|取得先|URL|
|---|---|
|**NuGet**| https://www.nuget.org/packages/MediSignVerifier<br/>(ライブラリのみ)|
|**GitHub**| https://github.com/findex-inc/MediSignVerifier/releases|


## プロジェクト(program source)
|取得先|URL|
|---|---|
|**EPDVerifyCmd**| コマンドによる電子署名検証の実施します。検証結果をXMLファイル形式のレポートを作成します。内部的にはMediSignVerifierをライブラリとして利用しています。 |
|**MediSignVerifier**| ライブラリ本体、署名検証処理はここに記載されています。|
|**MediSignVerifier.Tests**| MediSignVerifierに対する単体テスト用プロジェクトになります。|


## 使用方法
ライブラリの利用例を記載します。

```csharp
using System;
using System.Text;
using NLog;
using SignatureVerifier;

// <summary>検証メソッド</summary>/
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

// <summary>検証結果の受取</summary>/
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
## 制限事項
* 電子処方箋に使われているXAdES形式にのみ対応してます。
* ネットワークを介した検証は行っていません。
  
## 今後の予定
* 電子カルテ状況共有サービスに対する文書への対応
  

## 免責事項
本ソフトウェアは、現状のまま提供され、明示または黙示を問わず、いかなる種類の保証もありません。
本ソフトウェアの使用に起因または関連して発生したいかなる損害（直接的、間接的、偶発的、特別、結果的、懲罰的なものを含むがこれらに限定されない）についても、作者は一切の責任を負いません。

## 問い合わせ
本件ライブラリに対する問い合わせは、GitHub Issue(https://github.com/findex-inc/MediSignVerifier/issues)より行ってください。

