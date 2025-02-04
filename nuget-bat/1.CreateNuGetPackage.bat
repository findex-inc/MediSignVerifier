@echo off
echo ----------------------------------------
echo NuGet用パッケージの作成
echo ----------------------------------------
echo 【注意事項】
echo 　・[releaseNotes]が今回リリースのサマリ情報になっていることを確認してください
echo;

:menu
echo.
echo 1. SignatureVerifier
echo.

set /p choice=番号を選んでください (1):

if "%choice%"=="1" goto choice1
echo 不正な選択です。もう一度選んでください。
goto menu

:choice1
cd ..\src\SignatureVerifier
goto prebuild

:prebuild
dotnet pack
move bin\Release\*.nupkg ..\..\build\

echo;
SET /P end="何か入力すると終了します"