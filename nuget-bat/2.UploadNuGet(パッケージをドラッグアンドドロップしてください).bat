@echo off
echo ----------------------------------------
echo パッケージをNuGetサーバーにアップロード
echo ----------------------------------------
echo アップロードするパッケージ：%1
echo 【注意事項】
echo 　・NuGet.exeのパスが通っていることを確認してください。
echo 　・同じVerのパッケージはアップロードできません。NuGet監理者に問い合わせて同じVerのパッケージを削除してください
echo;
SET /P start="何か入力するとパッケージをNuGetサーバーにアップロードします"
nuget.exe push %1 FindexNuget -Source http://nuget.pscad.co.jp/nuget
SET /P end="何か入力すると終了します"