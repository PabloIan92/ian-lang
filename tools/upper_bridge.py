#!/usr/bin/env python3
import json
import sys


def main() -> int:
    raw = sys.stdin.read()
    data = json.loads(raw) if raw.strip() else ""
    print(json.dumps({"original": data, "mayusculas": str(data).upper()}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
