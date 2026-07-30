@echo off
setlocal
set "IAN_HOME=%~dp0"
python "%IAN_HOME%ian.py" %*
