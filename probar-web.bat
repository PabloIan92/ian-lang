@echo off
setlocal
set "IAN_HOME=%~dp0"
call "%IAN_HOME%ian.exe" "%IAN_HOME%examples\web.ian"
