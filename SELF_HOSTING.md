# i@N Self Hosting

El objetivo es que i@N deje de depender cada vez mas del codigo C# y mueva su
comportamiento a archivos `.ian`.

## Estado actual

`ian.exe` sigue siendo el motor base, escrito en C#/.NET. Desde `0.4.0`, el motor
ya soporta dos piezas necesarias para autohospedaje:

- `incluir "archivo.ian"` para cargar modulos i@N.
- `archivo leer|escribir|agregar|existe` para que i@N pueda leer y escribir su
  propio estado.

## Primer nucleo en i@N

La carpeta `self/` contiene las primeras partes de i@N escritas en i@N:

- `core.ian`
- `memory.ian`
- `reasoning.ian`
- `tools.ian`
- `bootstrap.ian`

Probar:

```powershell
ian ".\self\bootstrap.ian"
```

Eso genera:

```txt
self\build\self_manifest.txt
```

## Proximo paso

Mover mas logica desde ejecutables C# hacia modulos `.ian`: reglas de
clasificacion, respuestas humanas, perfiles expertos y plantillas de aprendizaje.
