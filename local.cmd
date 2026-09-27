@echo off
setlocal
set "IAN_HOME=%~dp0"
python "%IAN_HOME%local_brain.py" %*
