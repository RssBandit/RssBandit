@echo off

set msbuild2=C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\msbuild.exe
set msbuild35=C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild.exe

%msbuild35% MSBuildDefault.Proj /target:Installer /property:Configuration=Release /ToolsVersion:3.5

pause


