# i@N

i@N es un lenguaje experimental, en español, pensado para automatización técnica:
redes, web, programación rápida y comunicación entre programas escritos en otros
lenguajes.

La versión `0.5.0` unifica todos los módulos en un solo binario `ian.exe`.
No reemplaza a Java, Python o JavaScript todavía; los usa como fuerza de ejecución cuando conviene.

## Objetivo

- Sintaxis amigable para personas de redes, Mikrotik, soporte técnico, web y automatización.
- Comandos simples para red: DNS, puertos y ping.
- Generación básica de páginas HTML.
- Nexo entre lenguajes mediante procesos externos: i@N envía JSON por stdin y lee JSON/texto por stdout.
- Comando `agente` para planes estructurados fáciles de generar por IA.
- Base clara para crecer hacia un compilador, extensión de editor y librería estándar.

## Ejecutar

```powershell
cd "C:\Users\CONECTIA BA\Desktop\ian-lang"
.\ian.exe .\examples\hola.ian
.\ian.exe .\examples\web.ian
.\ian.exe --allow-run .\examples\nexo-python.ian
```

## Arquitectura: binario único

Desde v0.5.0, todos los módulos viven en un solo `ian.exe` (150 KB, .NET 8.0).

| Módulo | Comando | Rol |
|--------|---------|-----|
| Motor | `ian.exe <archivo.ian>` | Interpreta archivos `.ian` |
| Agent | `ian.exe agent ...` | Convierte pedidos a programas i@N |
| Brain | `ian.exe brain ...` | Memoria/aprendizaje local |
| Teacher | `ian.exe teacher ...` | Importa enseñanzas externas |
| Shell | `ian.exe shell` | Consola interactiva (`i@n`) |
| Supervisor | `ian.exe supervisor ...` | Revisor de calidad y seguridad |
| Human | `ian.exe human ...` | Capa conversacional y perfiles expertos |
| API | `ian.exe api ...` | Conexión con APIs externas |
| Daily | `ian.exe daily ...` | Aprendizaje diario automático |
| Architect | `ian.exe architect ...` | Mapa interno para evolucionar i@N |

Archivos fuente:
- `ian_engine.cs`: motor del lenguaje (v0.5.0)
- `ian_agent.cs`, `ian_brain_next.cs`, `ian_teacher.cs`, `ian_shell.cs`
- `ian_supervisor.cs`, `ian_human.cs`, `ian_api.cs`, `ian_daily.cs`, `ian_architect.cs`
- `ian_auto.cs`
- `Program.cs`: despachador de módulos

## Novedades v0.5.0

- **Binario único**: 11 ejecutables unificados en `ian.exe`
- **Módulos como subcomandos**: `ian.exe brain`, `ian.exe shell`, etc.
- **Brain v0.5.0**: `ian_brain_next.cs` reemplaza a `ian_brain.cs`
- **Backward compat**: wrappers `.cmd` para todos los nombres anteriores

## Self Hosting

i@N ya tiene una primera base escrita en i@N:

```powershell
ian ".\self\bootstrap.ian"
```

Eso usa `incluir` y `archivo` para cargar módulos propios y generar
`self\build\self_manifest.txt`.

## i@N Agent

```powershell
.\ian.exe agent --run "crea una pagina web para mi empresa y revisa el puerto http de example.com"
```

## i@N Brain

```powershell
.\ian.exe brain aprender "crear saludo" "decir \"Hola aprendido\""
.\ian.exe brain preguntar "quiero crear saludo"
.\ian.exe brain crear "crea una web simple"
```

## i@N Teacher

```powershell
.\ian.exe teacher plantillas
.\ian.exe teacher abrir
.\ian.exe teacher importar
.\ian.exe teacher pedir claude "enseñale a crear una pagina web"
.\ian.exe teacher pedir codex "enseñale a revisar un puerto"
.\ian.exe teacher vigilar
```

## Invocación Por Comando

```powershell
i@n
```

Abre una consola interactiva donde i@N pregunta qué querés hacer.

```powershell
ian.exe auto "crea una web simple"
```

## Ejemplo

```ian
funcion saludo nombre
  unir "Hola " $nombre como resultado
fin

llamar saludo "Mundo" como mi_mensaje
decir $mi_mensaje

web iniciar "Demo i@N" como pagina
web titulo pagina "Pagina generada con i@N"
web texto pagina "Esto salio desde el nuevo lenguaje."
web boton pagina "Probar" mensaje "i@N esta funcionando"
web guardar pagina en "salida/demo.html"
```

## Interoperabilidad

Un nexo es un proceso externo. Puede ser Python, Node, PowerShell, Go, Java, etc.

```ian
nexo mayusculas = proceso "python ../tools/upper_bridge.py"
llamar mayusculas con "hola mikrotik" como respuesta
decir $respuesta
```

El programa externo recibe JSON por stdin y debe imprimir JSON o texto por stdout.

## Estado

Implementado (v0.5.0):

- `decir` (múltiples argumentos), `guardar`
- `sumar`, `restar`, `multiplicar`, `dividir`, `modulo`, `incrementar`, `decrementar`
- `repetir ... veces` / `fin`
- `repetir para cada ... en ...` / `fin`
- `si ... entonces` / `sino` / `fin` con 7 operadores de comparación
- `si archivo existe ...`
- `mientras ... fin` (límite 100.000 iteraciones)
- `funcion` / `fin` y `llamar`
- `texto longitud`, `texto mayusculas`, `texto minusculas`, `texto contiene`, `texto recortar`
- `unir` (concatenación N-aria)
- `lista crear`, `lista agregar`, `lista obtener`, `lista tamanno`, `lista json`
- `red dns`, `red puerto`, `red ping`
- `web iniciar`, `web titulo`, `web texto`, `web boton`, `web guardar`
- `api get`
- `mikrotik script`
- `nexo` y `llamar`
- `agente plan`, `agente paso`, `agente json`
- `archivo leer`, `archivo escribir`, `archivo agregar`, `archivo existe`
- `incluir` (ejecutar otro .ian en el mismo contexto)

Pendiente para versiones futuras:

- Tipos declarados
- Paquetes e importación de módulos
- Plugin real para RouterOS API (SSH/API nativa)
- Transpilador a JavaScript para web y aplicaciones móviles

## Evolución y Personalidad

i@N no es un sistema estático. Podés enseñarle a ser menos 'robotico' y mas humano:
- Usá `ian.exe brain aprender "pregunta" "ia decir \"respuesta\""` para darle nuevas frases.
- Usá `ian.exe teacher pedir claude "enseñale a i@n a ser mas natural"` para importar lecciones de personalidad.
- El archivo `brain/memory.tsv` es su 'alma' digital; podés editarlo para cambiar su conducta.
