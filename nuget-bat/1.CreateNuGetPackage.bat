@echo off
echo ----------------------------------------
echo NuGet�p�p�b�P�[�W�̍쐬
echo ----------------------------------------
echo �y���ӎ����z
echo �@�E[releaseNotes]�����񃊃��[�X�̃T�}�����ɂȂ��Ă��邱�Ƃ��m�F���Ă�������
echo;

:menu
echo.
echo 1. SignatureVerifier
echo.

set /p choice=�ԍ���I��ł������� (1):

if "%choice%"=="1" goto choice1
echo �s���ȑI���ł��B������x�I��ł��������B
goto menu

:choice1
cd ..\src\SignatureVerifier
goto prebuild

:prebuild
dotnet pack
move bin\Release\*.nupkg ..\..\build\

echo;
SET /P end="�������͂���ƏI�����܂�"