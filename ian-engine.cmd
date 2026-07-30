@echo off
setlocal
set "IAN_HOME=%~dp0"
"%IAN_HOME%ian.exe" engine %*
