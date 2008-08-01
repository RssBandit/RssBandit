@echo off

set dvenv="%ProgramFiles%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com"

set CONFIG=%1
IF /i "X%CONFIG%" EQU "X" SET CONFIG=Release

IF /i "%PROCESSOR_ARCHITECTURE%" EQU "AMD64" SET set dvenv="%ProgramFiles(x86)%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com"

%dvenv% "source\RSS Bandit.2008.sln" /build %1 /project "RssBandit Installer"

pause


