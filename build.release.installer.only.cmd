@echo off

set dvenv="C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com"

set CONFIG=%1
IF /i "X%CONFIG%" EQU "X" SET CONFIG=Release

%dvenv% "source\RSS Bandit.2008.sln" /build %1 /project "RssBandit Installer"

pause


