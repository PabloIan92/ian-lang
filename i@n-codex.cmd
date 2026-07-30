@echo off
setlocal
set "IAN_HOME=%~dp0"
echo Abriendo puente de enseñanza Codex -> i@N
echo 1. Ejecuta: codex
echo 2. Copia el prompt: %IAN_HOME%teach-inbox\PROMPT_CODEX.txt
echo 3. Guarda la respuesta en: %IAN_HOME%teach-inbox\codex
echo 4. Importa con: i@n-teacher importar
"%IAN_HOME%ian.exe" teacher plantillas
