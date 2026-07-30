# Enseñanza Automatica

Ahora i@N tiene dos formas mas simples de aprender de Claude o Codex.

## Desde la consola viva

```powershell
i@n
```

Dentro de i@N:

```txt
claude enseñale a crear una pagina web para una empresa
codex enseñale a revisar el puerto http de un servidor
```

i@N intenta llamar al comando externo, guardar la respuesta como `.teach` e
importarla a `i@N Brain`.

## Vigilancia automatica

En una PowerShell separada:

```powershell
ian-teacher vigilar
```

Despues cualquier archivo `.teach` o `.ian` que aparezca en estas carpetas se
importa solo:

- `teach-inbox\claude`
- `teach-inbox\codex`
- `teach-inbox\manual`

## Si Claude o Codex se abren en modo interactivo

Algunas instalaciones no aceptan prompts automaticos. En ese caso:

1. Ejecutar `i@n-claude` o `i@n-codex`.
2. Pegar el prompt en Claude/Codex.
3. Guardar la respuesta en la bandeja correspondiente.
4. Si `ian-teacher vigilar` esta corriendo, i@N aprende solo.
