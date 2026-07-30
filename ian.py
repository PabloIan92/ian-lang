#!/usr/bin/env python3
"""
i@N language runtime.

Version 0.1 is a practical line-oriented interpreter for automation,
network checks, simple web page generation, and inter-process bridges.
"""

from __future__ import annotations

import argparse
import html
import json
import os
import shlex
import socket
import subprocess
import sys
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Any


VERSION = "0.1.0"


class IanError(Exception):
    pass


@dataclass
class Bridge:
    name: str
    command: list[str]


@dataclass
class Page:
    title: str
    parts: list[str]


@dataclass
class AgentPlan:
    goal: str
    steps: list[dict[str, str]]


class Runtime:
    def __init__(self, base_dir: Path, allow_run: bool = False) -> None:
        self.base_dir = base_dir
        self.allow_run = allow_run
        self.vars: dict[str, Any] = {}
        self.bridges: dict[str, Bridge] = {}

    def run_file(self, path: Path) -> None:
        text = path.read_text(encoding="utf-8")
        self.run(text, path)

    def run(self, source: str, path: Path | None = None) -> None:
        lines = source.splitlines()
        i = 0
        while i < len(lines):
            raw = lines[i]
            line = clean_line(raw)
            i += 1
            if not line:
                continue
            if line.startswith("repetir "):
                block, i = self._collect_block(lines, i, "fin")
                count = self._parse_repeat_count(line)
                for _ in range(count):
                    self.run("\n".join(block), path)
                continue
            self.execute(line)

    def execute(self, line: str) -> None:
        parts = split(line)
        if not parts:
            return
        head = parts[0].lower()
        if head == "decir":
            print(self.value(" ".join(parts[1:])))
            return
        if head == "guardar":
            self._cmd_guardar(parts)
            return
        if head == "sumar":
            self._cmd_sumar(parts)
            return
        if head == "red":
            self._cmd_red(parts)
            return
        if head == "web":
            self._cmd_web(parts)
            return
        if head == "nexo":
            self._cmd_nexo(parts)
            return
        if head == "llamar":
            self._cmd_llamar(parts)
            return
        if head == "api":
            self._cmd_api(parts)
            return
        if head == "mikrotik":
            self._cmd_mikrotik(parts)
            return
        if head == "agente":
            self._cmd_agente(parts)
            return
        raise IanError(f"Comando desconocido: {parts[0]}")

    def value(self, token_text: str) -> Any:
        token_text = token_text.strip()
        if not token_text:
            return ""
        if token_text.startswith("$"):
            name = token_text[1:]
            if name not in self.vars:
                raise IanError(f"Variable no definida: {name}")
            return self.vars[name]
        try:
            return json.loads(token_text)
        except json.JSONDecodeError:
            pass
        if token_text.lower() == "verdadero":
            return True
        if token_text.lower() == "falso":
            return False
        if token_text.lower() == "nulo":
            return None
        return token_text

    def _cmd_guardar(self, parts: list[str]) -> None:
        if len(parts) < 4 or parts[2] != "=":
            raise IanError('Uso: guardar nombre = "valor"')
        name = parts[1]
        self.vars[name] = self.value(" ".join(parts[3:]))

    def _cmd_sumar(self, parts: list[str]) -> None:
        if "como" not in parts:
            raise IanError("Uso: sumar a b como resultado")
        idx = parts.index("como")
        if idx != 3 or len(parts) != 5:
            raise IanError("Uso: sumar a b como resultado")
        self.vars[parts[4]] = float(self.value(parts[1])) + float(self.value(parts[2]))

    def _cmd_red(self, parts: list[str]) -> None:
        if len(parts) < 5 or "como" not in parts:
            raise IanError("Uso: red dns dominio como variable | red puerto host puerto como variable")
        action = parts[1].lower()
        out = parts[-1]
        if parts[-2] != "como":
            raise IanError("Falta 'como variable'")
        if action == "dns":
            host = str(self.value(parts[2]))
            try:
                name, aliases, addresses = socket.gethostbyname_ex(host)
                self.vars[out] = {"ok": True, "nombre": name, "alias": aliases, "ips": addresses}
            except OSError as exc:
                self.vars[out] = {"ok": False, "error": str(exc)}
            return
        if action == "puerto":
            host = str(self.value(parts[2]))
            port = int(self.value(parts[3]))
            self.vars[out] = tcp_open(host, port)
            return
        if action == "ping":
            host = str(self.value(parts[2]))
            self.vars[out] = self._ping(host)
            return
        raise IanError(f"Accion de red desconocida: {action}")

    def _cmd_web(self, parts: list[str]) -> None:
        if len(parts) < 2:
            raise IanError("Uso: web iniciar|titulo|texto|boton|guardar")
        action = parts[1].lower()
        if action == "iniciar":
            if len(parts) != 5 or parts[3] != "como":
                raise IanError('Uso: web iniciar "Titulo" como pagina')
            self.vars[parts[4]] = Page(str(self.value(parts[2])), [])
            return
        if action in {"titulo", "texto"}:
            if len(parts) < 4:
                raise IanError(f"Uso: web {action} pagina texto")
            page = self._page(parts[2])
            tag = "h1" if action == "titulo" else "p"
            page.parts.append(f"<{tag}>{html.escape(str(self.value(' '.join(parts[3:]))))}</{tag}>")
            return
        if action == "boton":
            if len(parts) < 6 or parts[4] != "mensaje":
                raise IanError('Uso: web boton pagina "Etiqueta" mensaje "Texto"')
            page = self._page(parts[2])
            label = html.escape(str(self.value(parts[3])))
            message = json.dumps(str(self.value(" ".join(parts[5:]))))
            page.parts.append(f'<button onclick="alert({message})">{label}</button>')
            return
        if action == "guardar":
            if len(parts) != 5 or parts[3] != "en":
                raise IanError('Uso: web guardar pagina en "archivo.html"')
            page = self._page(parts[2])
            target = self._safe_path(str(self.value(parts[4])))
            target.write_text(render_page(page), encoding="utf-8")
            print(f"web: {target}")
            return
        raise IanError(f"Accion web desconocida: {action}")

    def _cmd_nexo(self, parts: list[str]) -> None:
        if len(parts) < 5 or parts[2] != "=" or parts[3] != "proceso":
            raise IanError('Uso: nexo nombre = proceso "python script.py"')
        name = parts[1]
        command = shlex.split(str(self.value(" ".join(parts[4:]))), posix=False)
        if not command:
            raise IanError("El nexo necesita un comando")
        self.bridges[name] = Bridge(name, command)

    def _cmd_llamar(self, parts: list[str]) -> None:
        if len(parts) < 6 or parts[2] != "con" or parts[-2] != "como":
            raise IanError('Uso: llamar nexo con "dato" como variable')
        name = parts[1]
        if name not in self.bridges:
            raise IanError(f"Nexo no definido: {name}")
        if not self.allow_run:
            raise IanError("Ejecutar procesos externos requiere --allow-run")
        payload = self.value(" ".join(parts[3:-2]))
        out_name = parts[-1]
        proc = subprocess.run(
            self.bridges[name].command,
            input=json.dumps(payload, ensure_ascii=False),
            text=True,
            capture_output=True,
            cwd=str(self.base_dir),
            check=False,
        )
        if proc.returncode != 0:
            raise IanError(proc.stderr.strip() or f"Nexo fallo con codigo {proc.returncode}")
        self.vars[out_name] = parse_output(proc.stdout)

    def _cmd_api(self, parts: list[str]) -> None:
        if len(parts) != 5 or parts[1] != "get" or parts[3] != "como":
            raise IanError('Uso: api get "https://..." como respuesta')
        url = str(self.value(parts[2]))
        with urllib.request.urlopen(url, timeout=10) as response:
            body = response.read().decode("utf-8", errors="replace")
        self.vars[parts[4]] = parse_output(body)

    def _cmd_mikrotik(self, parts: list[str]) -> None:
        if len(parts) < 5 or parts[1] != "script" or parts[-2] != "como":
            raise IanError('Uso: mikrotik script "/ip address print" como variable')
        command = str(self.value(" ".join(parts[2:-2])))
        self.vars[parts[-1]] = {
            "routeros": command,
            "nota": "i@N v0.1 genera el script; la conexion real se integra por nexo externo.",
        }

    def _cmd_agente(self, parts: list[str]) -> None:
        if len(parts) < 2:
            raise IanError("Uso: agente plan|paso|json")
        action = parts[1].lower()
        if action == "plan":
            if len(parts) < 5 or parts[-2] != "como":
                raise IanError('Uso: agente plan "objetivo" como nombre')
            self.vars[parts[-1]] = AgentPlan(str(self.value(" ".join(parts[2:-2]))), [])
            return
        if action == "paso":
            if len(parts) < 5:
                raise IanError('Uso: agente paso plan hacer "accion"')
            plan = self._plan(parts[2])
            status = parts[3].lower()
            if status not in {"hacer", "haciendo", "hecho"}:
                raise IanError("Estado de paso invalido: usar hacer, haciendo o hecho")
            plan.steps.append({"estado": status, "accion": str(self.value(" ".join(parts[4:])))})
            return
        if action == "json":
            if len(parts) != 5 or parts[3] != "como":
                raise IanError("Uso: agente json plan como variable")
            plan = self._plan(parts[2])
            self.vars[parts[4]] = {"objetivo": plan.goal, "pasos": plan.steps}
            return
        raise IanError(f"Accion agente desconocida: {action}")

    def _page(self, name: str) -> Page:
        value = self.vars.get(name)
        if not isinstance(value, Page):
            raise IanError(f"No es una pagina web: {name}")
        return value

    def _plan(self, name: str) -> AgentPlan:
        value = self.vars.get(name)
        if not isinstance(value, AgentPlan):
            raise IanError(f"No es un plan agente: {name}")
        return value

    def _safe_path(self, name: str) -> Path:
        target = (self.base_dir / name).resolve()
        if not str(target).startswith(str(self.base_dir.resolve())):
            raise IanError("La salida debe quedar dentro de la carpeta del programa")
        target.parent.mkdir(parents=True, exist_ok=True)
        return target

    def _ping(self, host: str) -> bool:
        if not self.allow_run:
            raise IanError("red ping requiere --allow-run porque usa el comando del sistema")
        flag = "-n" if os.name == "nt" else "-c"
        proc = subprocess.run(["ping", flag, "1", host], capture_output=True, text=True)
        return proc.returncode == 0

    def _collect_block(self, lines: list[str], start: int, end_word: str) -> tuple[list[str], int]:
        block: list[str] = []
        i = start
        depth = 0
        while i < len(lines):
            line = clean_line(lines[i])
            i += 1
            if line.startswith("repetir "):
                depth += 1
            if line == end_word:
                if depth == 0:
                    return block, i
                depth -= 1
            block.append(lines[i - 1])
        raise IanError(f"Bloque sin cierre: {end_word}")

    def _parse_repeat_count(self, line: str) -> int:
        parts = split(line)
        if len(parts) != 3 or parts[2] != "veces":
            raise IanError("Uso: repetir 3 veces ... fin")
        return int(self.value(parts[1]))


def clean_line(line: str) -> str:
    stripped = line.strip()
    if not stripped or stripped.startswith("#"):
        return ""
    return stripped


def split(line: str) -> list[str]:
    return shlex.split(line, posix=False)


def tcp_open(host: str, port: int) -> bool:
    try:
        with socket.create_connection((host, port), timeout=3):
            return True
    except OSError:
        return False


def parse_output(text: str) -> Any:
    text = text.strip()
    if not text:
        return ""
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return text


def render_page(page: Page) -> str:
    return f"""<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{html.escape(page.title)}</title>
  <style>
    body {{ font-family: system-ui, sans-serif; max-width: 860px; margin: 40px auto; padding: 0 18px; }}
    button {{ padding: 10px 14px; border: 1px solid #1f2937; border-radius: 6px; background: #111827; color: white; }}
  </style>
</head>
<body>
  {chr(10).join(page.parts)}
</body>
</html>
"""


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="ian", description="Runtime del lenguaje i@N")
    parser.add_argument("file", nargs="?", help="Archivo .ian a ejecutar")
    parser.add_argument("--allow-run", action="store_true", help="Permite ejecutar nexos y ping del sistema")
    parser.add_argument("--version", action="store_true", help="Muestra la version")
    args = parser.parse_args(argv)
    if args.version:
        print(f"i@N {VERSION}")
        return 0
    if not args.file:
        parser.print_help()
        return 0
    path = Path(args.file).resolve()
    try:
        Runtime(path.parent, allow_run=args.allow_run).run_file(path)
    except IanError as exc:
        print(f"i@N error: {exc}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
