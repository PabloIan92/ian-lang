import json
import os
import sys
import urllib.error
import urllib.request


API_URL = "https://api.deepseek.com/chat/completions"


def main():
    prompt = " ".join(sys.argv[1:]).strip()
    if not prompt and not sys.stdin.isatty():
        prompt = sys.stdin.read().strip()
    if not prompt:
        print("Uso: deepseek \"pregunta\"")
        print("Antes configura: setx DEEPSEEK_API_KEY \"tu_api_key\"")
        return 2

    api_key = os.environ.get("DEEPSEEK_API_KEY", "").strip()
    if not api_key:
        print("Falta DEEPSEEK_API_KEY.")
        print("Configuralo con: setx DEEPSEEK_API_KEY \"tu_api_key\"")
        return 2

    model = os.environ.get("DEEPSEEK_MODEL", "deepseek-chat").strip() or "deepseek-chat"
    payload = {
        "model": model,
        "messages": [
            {
                "role": "system",
                "content": "Responde en espanol claro. Se concreto y util.",
            },
            {"role": "user", "content": prompt},
        ],
        "temperature": 0.3,
    }

    request = urllib.request.Request(
        API_URL,
        data=json.dumps(payload).encode("utf-8"),
        headers={
            "Authorization": "Bearer " + api_key,
            "Content-Type": "application/json",
            "User-Agent": "ian-lang-deepseek/1.0",
        },
        method="POST",
    )

    try:
        with urllib.request.urlopen(request, timeout=120) as response:
            data = json.loads(response.read().decode("utf-8"))
    except urllib.error.HTTPError as exc:
        body = exc.read().decode("utf-8", errors="replace")
        print("DeepSeek HTTP error:", exc.code)
        print(body)
        return 1
    except Exception as exc:
        print("DeepSeek error:", exc)
        return 1

    try:
        print(data["choices"][0]["message"]["content"].strip())
    except Exception:
        print(json.dumps(data, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
