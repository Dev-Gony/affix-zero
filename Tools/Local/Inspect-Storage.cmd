@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Inspect-LegacyStorage.ps1"
if errorlevel 1 echo Inspection failed. No cleanup action was requested.
pause
