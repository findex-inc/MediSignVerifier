@echo off
echo ----------------------------------------
echo �p�b�P�[�W��NuGet�T�[�o�[�ɃA�b�v���[�h
echo ----------------------------------------
echo �A�b�v���[�h����p�b�P�[�W�F%1
echo �y���ӎ����z
echo �@�ENuGet.exe�̃p�X���ʂ��Ă��邱�Ƃ��m�F���Ă��������B
echo �@�E����Ver�̃p�b�P�[�W�̓A�b�v���[�h�ł��܂���BNuGet�ė��҂ɖ₢���킹�ē���Ver�̃p�b�P�[�W���폜���Ă�������
echo;
SET /P start="�������͂���ƃp�b�P�[�W��NuGet�T�[�o�[�ɃA�b�v���[�h���܂�"
nuget.exe push %1 FindexNuget -Source http://nuget.pscad.co.jp/nuget
SET /P end="�������͂���ƏI�����܂�"