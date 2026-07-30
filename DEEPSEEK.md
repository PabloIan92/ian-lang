# DeepSeek en i@N

DeepSeek quedo instalado como comando local:

```powershell
deepseek "explicame derivadas en dos ejemplos"
```

Requiere una API key:

```powershell
setx DEEPSEEK_API_KEY "tu_api_key"
```

Cierra y abre la consola despues de usar `setx`.

Modelo por defecto:

```txt
deepseek-chat
```

Opcional:

```powershell
setx DEEPSEEK_MODEL "deepseek-chat"
```

## Como profesor de i@N

Desde `i@n`:

```txt
deepseek ensenale a i@n a razonar mejor
```

O directo:

```powershell
ian-teacher pedir deepseek "ensenale a i@n una regla de matematica"
```

i@N no copia el modelo interno de DeepSeek. Usa DeepSeek como profesor externo para extraer reglas, ejemplos y buenas practicas, y guardarlas en memoria local.
