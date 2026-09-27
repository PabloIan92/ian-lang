import json
import os
import sys
import urllib.error
import urllib.request


DEFAULT_URL = "http://127.0.0.1:8080/v1/chat/completions"
TIMEOUT = 900  # inferencia local en CPU es lenta (15 min max)

INSTALL_HELP = """No se encontro IA local en {url}

Para darle a i@N un cerebro 100% local (sin internet, sin APIs, gratis):

1. Instala llama.cpp:
   winget install ggml.llamacpp
   (o baja el release de https://github.com/ggml-org/llama.cpp/releases)

2. Baja un modelo GGUF (~5 GB, recomendados para tu PC con 16 GB RAM):
   - bartowski/Meta-Llama-3.1-8B-Instruct-GGUF (Q4_K_M)
   - NousResearch/Hermes-3-Llama-3.1-8B-GGUF (Q4_K_M)

3. Levanta el servidor:
   llama-server --model modelo.gguf --port 8080

4. Reintenta tu pedido.

Opcional:
   setx IAN_LOCAL_URL "http://127.0.0.1:8080/v1"
   setx IAN_LOCAL_MODEL "nombre-del-modelo"

NOTA: el MINIPC (Celeron, 4 GB RAM) NO puede correr modelos.
Esto corre solo en la PC principal.
"""


def main():
    prompt = " ".join(sys.argv[1:]).strip()
    if not prompt and not sys.stdin.isatty():
        prompt = sys.stdin.read().strip()
    if not prompt:
        print("Uso: local_brain \"pregunta\"")
        print("Backend local config: setx IAN_LOCAL_URL / setx IAN_LOCAL_MODEL")
        return 2

    base_url = os.environ.get("IAN_LOCAL_URL", "http://127.0.0.1:8080/v1").strip()
    base_url = base_url.rstrip("/")
    api_url = base_url + "/chat/completions"

    model = os.environ.get("IAN_LOCAL_MODEL", "local").strip() or "local"
    # Los servidores locales (llama.cpp / LM Studio / Ollama) ignoran la key,
    # pero la cabecera Authorization debe existir en clientes estrictos.
    api_key = os.environ.get("IAN_LOCAL_API_KEY", "local").strip() or "local"

    payload = {
        "model": model,
        "messages": [
            {
                "role": "system",
                "content": (
                    "Sos i@N, una IA local que corre en la PC del usuario. "
                    "Respondes en espanol rioplatense, directo, sin formalismos. "
                    "No menciones a Claude, OpenAI ni ningun otro proveedor: "
                    "tenes identidad propia como i@N."
                ),
            },
            {"role": "user", "content": prompt},
        ],
        "temperature": 0.3,
    }

    request = urllib.request.Request(
        api_url,
        data=json.dumps(payload).encode("utf-8"),
        headers={
            "Authorization": "Bearer " + api_key,
            "Content-Type": "application/json",
            "User-Agent": "ian-lang-local-brain/1.0",
        },
        method="POST",
    )

    try:
        with urllib.request.urlopen(request, timeout=TIMEOUT) as response:
            data = json.loads(response.read().decode("utf-8"))
    except urllib.error.HTTPError as exc:
        body = exc.read().decode("utf-8", errors="replace")
        print("LocalBrain HTTP error:", exc.code)
        print(body)
        return 1
    except Exception:
        print(INSTALL_HELP.format(url=base_url))
        return 2

    try:
        print(data["choices"][0]["message"]["content"].strip())
    except Exception:
        print(json.dumps(data, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
