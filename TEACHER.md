# i@N Teacher

`i@N Teacher` es la interfaz de enseñanza para que Codex, Claude o una persona
puedan alimentar a `i@N Brain` sin convertirlo en dependiente de una IA externa.

## Idea

Codex o Claude actuan como profesores externos. Generan archivos `.teach` o
`.ian`. Despues `ian-teacher.exe` importa esos archivos a la memoria local de
`i@N Brain`.

## Carpetas

- `teach-inbox/codex`: respuestas preparadas por Codex.
- `teach-inbox/claude`: respuestas preparadas por Claude.
- `teach-inbox/manual`: enseñanzas escritas por una persona.
- `teach-inbox/aprendido`: archivos ya importados.

## Crear plantillas

```powershell
.\ian-teacher.exe plantillas
```

Esto crea prompts listos para copiar y pegar en Codex o Claude.

## Importar enseñanzas

```powershell
.\ian-teacher.exe importar
```

## Formato `.teach`

```ian
# pregunta: crear una pagina web para una empresa
agente plan "Crear web" como plan
web iniciar "Empresa" como pagina
web titulo pagina "Empresa"
web guardar pagina en "salida/empresa.html"
```

## Panel visual

Abrir:

```powershell
.\ian-teacher.exe abrir
```
