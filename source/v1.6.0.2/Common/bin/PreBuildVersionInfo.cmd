@echo off
rem --------------------------------------------------------------
rem Call PrebuildVersionInfo.cmd 
rem Call PrebuildVersionInfo.cmd with "<my_fileversion_path>"
rem --------------------------------------------------------------
cScript.exe //E:vbscript "D:\My Projects\DOT.NET\Sourceforge.RssBandit\CurrentWork\Source\Common\bin\GenerateVersionInfo.vbs" %1 %2 > prebuild.report.txt

