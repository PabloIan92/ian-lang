# i@N Autonomy

`i@N Auto` es el primer ciclo autonomo local:

1. Recibe un objetivo.
2. Consulta `i@N Brain`.
3. Crea un programa `.ian`.
4. Ejecuta el motor `ian.exe`.
5. Guarda un registro en `autonomy-logs`.

Uso:

```powershell
ian-auto "crea una web simple"
i@n
autonomo crea una web simple
```

Con ayuda externa opcional:

```powershell
ian-auto --ask-external "mejora la forma de crear reportes de red"
```

## Principio

i@N no debe depender de otra IA para vivir. Puede pedir clases a Codex o Claude,
pero lo aprendido queda convertido en memoria local y codigo i@N auditable.

## Limite actual

Esta autonomia todavia es pequena. No razona como un modelo neuronal grande, pero
ya tiene un ciclo propio de objetivo, plan, ejecucion, memoria y registro.
