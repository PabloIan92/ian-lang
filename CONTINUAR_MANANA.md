# Continuar i@N mañana

Fecha de nota: 2026-07-29

## Objetivo principal

El usuario quiere que i@N evolucione hacia una inteligencia propia local, no una
simple interfaz hacia Codex, Claude u otra IA. La idea central es:

- i@N debe tener vida propia.
- i@N debe aprender de Codex/Claude solo como profesores externos.
- i@N debe convertir lo aprendido en memoria propia local.
- i@N debe dejar de sonar robotico.
- i@N debe avanzar hacia autohospedaje: partes de i@N escritas en i@N.

## Estado actual

Carpeta principal:

```txt
C:\Users\CONECTIA BA\Desktop\ian-lang
```

Comando principal:

```powershell
i@n
```

Motor actual:

```powershell
ian.exe --version
```

Debe mostrar:

```txt
i@N 0.5.0
```

## Arquitectura actual (v0.5.0)

Todo es un solo binario `ian.exe` (150 KB). Cada módulo se invoca con el nombre
como primer argumento:

| Módulo | Invocación |
|--------|-----------|
| Motor | `ian.exe <archivo.ian>` |
| Shell | `ian.exe shell` |
| Brain | `ian.exe brain ...` |
| Agent | `ian.exe agent ...` |
| Teacher | `ian.exe teacher ...` |
| Human | `ian.exe human ...` |
| Auto | `ian.exe auto ...` |
| API | `ian.exe api ...` |
| Daily | `ian.exe daily ...` |
| Supervisor | `ian.exe supervisor ...` |
| Architect | `ian.exe architect ...` |

Wrapper `.cmd` existentes para compatibilidad hacia atrás (redirigen a `ian.exe <módulo>`):

```powershell
i@n
ian
ian-agent
ian-brain
ian-teacher
ian-api
ian-daily
ian-auto
ian-supervisor
ian-human
ian-architect
ian-self
i@n-codex
i@n-claude
deepseek
```

## Autohospedaje

Ya existe una primera base escrita en i@N:

```txt
self/core.ian
self/memory.ian
self/reasoning.ian
self/tools.ian
self/bootstrap.ian
```

Probar:

```powershell
ian-self
```

O dentro de `i@n`:

```txt
autohospedaje
```

Genera:

```txt
self/build/self_manifest.txt
```

El motor ya soporta:

```ian
incluir "archivo.ian"
archivo leer "datos.txt" como contenido
archivo escribir "salida.txt" "texto"
archivo agregar "salida.txt" "mas texto"
archivo existe "salida.txt" como existe
```

## Aprendizaje de Codex y Claude

i@N puede pedir enseñanzas:

```powershell
ian.exe teacher pedir codex "enseñale a i@n una regla simple"
ian.exe teacher pedir claude "enseñale a i@n una regla simple"
```

Importante: no se puede copiar el modelo interno de Codex/Claude. Lo correcto es
pedirles ejemplos, reglas y buenas practicas, y convertir eso en memoria local.

## Conversacion humana

El usuario se quejo con razon de que i@N sonaba robotico.

Se simplifico el saludo de `i@n`:

```txt
Hola, soy i@N.
Decime que queres hacer, o hablame normal. Si necesitas opciones, escribi ayuda.
i@N>
```

Se reescribio `ian_human.cs` para responder mejor a:

```txt
podes hablar normal_?
que modelo de programacion se uso para crearte?
por algun motivo no queres que tenga vida propia como vos?
```

## API externa

Configurada API `wiki`:

```powershell
ian.exe api listar
ian.exe api preguntar wiki "MikroTik"
```

Dentro de `i@n`:

```txt
api preguntar wiki MikroTik
```

Se corrigio TLS 1.2 en `ian_api.cs`.

## Aprendizaje diario

Existe tarea programada de Windows:

```txt
iN_Daily_Learn
```

Corre todos los dias a las 09:00.

Manual:

```powershell
ian.exe daily
ian.exe daily "MikroTik"
```

Si hay variables `GOOGLE_API_KEY` y `GOOGLE_CX`, `ian.exe daily` puede usar Google
Custom Search. Si no, usa Wikipedia.

## Arquitectura interna

`ian.exe architect` define las piezas necesarias para que i@N crezca:

- entrada
- clasificador
- memoria
- planificador
- herramientas
- supervisor
- aprendizaje
- autonomia
- personalidad
- seguridad

Comandos:

```powershell
ian.exe architect mapa
ian.exe architect roadmap
ian.exe architect sembrar
ian.exe architect siguiente
```

## Proximo paso recomendado

Prioridad:

1. Mover mas logica desde C# hacia archivos `.ian` en `self/`.
2. Crear `self/human.ian` con reglas conversacionales editables en i@N.
3. Crear `self/classifier.ian` para clasificar pedidos: charla, accion, aprendizaje, API, autohospedaje.
4. Crear `self/teacher.ian` con reglas para limpiar enseñanzas de Codex/Claude.
5. Mejorar `ian.exe brain` para memoria con puntuacion por relevancia, fecha, fuente, calidad y validacion.
6. Hacer que `ian.exe auto` use `ian.exe supervisor` antes de ejecutar.
7. Hacer que `i@n` deje de depender tanto de respuestas fijas.

## Cuidado importante

No prometer que i@N ya es una IA superior. Todavia no lo es. El enfoque correcto
es construir capacidad real:

- entender mejor,
- preguntar cuando no sabe,
- consultar API,
- aprender de profesores externos,
- guardar memoria,
- revisar calidad,
- ejecutar acciones,
- registrar logs,
- mover comportamiento a `.ian`.

## Frase guia del usuario

> Primero entende como estas hecho, y luego anda creando cada parte que vos tenes
> pero en una version superior.

Traducido a trabajo tecnico:

Crear versiones i@N de cada pieza de la arquitectura del asistente, una por una,
empezando por self-hosting, clasificacion, memoria y conversacion.
