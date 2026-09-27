# IA local en i@N

i@N puede aprender sin depender de ninguna API externa.
El modulo `local_brain` habla con un servidor de IA que corre en tu propia PC.

```powershell
local "explicame subredes con un ejemplo"
```

## Requisitos

1. **llama.cpp** (gratis, open source):

```powershell
winget install ggml.llamacpp
```

o el release desde https://github.com/ggml-org/llama.cpp/releases

Tambien sirven LM Studio u Ollama si exponen API OpenAI-compatible
en `http://127.0.0.1:8080/v1`.

2. **Un modelo GGUF** (~5 GB). Recomendados para tu PC (i5, 16 GB RAM, sin GPU):

| Modelo | Tamano | Fuerte en |
|--------|--------|-----------|
| Llama-3.1-8B-Instruct Q4_K_M | ~4.9 GB | General |
| Hermes-3-Llama-3.1-8B Q4_K_M | ~4.9 GB | Instrucciones, JSON |
| Qwen2.5-Coder-7B Q4_K_M | ~4.7 GB | Codigo |

3. **Levantar el servidor**:

```powershell
llama-server --model .\modelo.gguf --port 8080 --ctx-size 8192
```

## Configuracion opcional

```powershell
setx IAN_LOCAL_URL "http://127.0.0.1:8080/v1"
setx IAN_LOCAL_MODEL "local"
```

Cierra y abre la consola despues de `setx`.

## Como profesor de i@N

```powershell
ian-teacher pedir local "ensenale a i@n a revisar un puerto"
```

i@N guarda lo aprendido en `brain/memory.tsv`. El conocimiento queda
en tu PC para siempre: sin internet, sin costo, sin terceros.

## Advertencia de hardware

La PC principal (16 GB RAM) corre modelos 7B/8B cuantizados a Q4.
El **MINIPC (Celeron 847E, 4 GB RAM) NO puede correrlos**: queda
como servidor web, no como cerebro de i@N.
