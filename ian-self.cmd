@echo off
setlocal
set "IAN_HOME=%~dp0"
"%IAN_HOME%ian.exe" "%IAN_HOME%self\bootstrap.ian"
