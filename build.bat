@echo off
rem ONAIR build — .NET Framework C# compiler (ships with Windows, no SDK needed)
setlocal
set CSC=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe

"%CSC%" /nologo /codepage:65001 /target:winexe /out:ONAIR.exe /win32icon:onair.ico ^
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ^
  /reference:Microsoft.VisualBasic.dll ONAIR.cs

if errorlevel 1 ( echo Build FAILED. & exit /b 1 )
echo Build OK -^> ONAIR.exe
