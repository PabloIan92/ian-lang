@chcp 65001 >nul
setlocal
set "IAN_HOME=%~dp0"
"%IAN_HOME%ian.exe" shell %*

