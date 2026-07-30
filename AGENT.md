# i@N Agent

`i@N Agent` es la primera capa inteligente local del proyecto.

No es una IA neuronal completa. Es un agente inicial basado en reglas que toma
pedidos en español, detecta intenciones y genera un programa `.ian`.

## Ejecutar

```powershell
cd "C:\Users\CONECTIA BA\Desktop\ian-lang"
.\ian-agent.exe "crea una pagina web para mi empresa"
.\ian-agent.exe --run "crea una pagina web y revisa el puerto http de example.com"
.\ian-agent.exe --chat --run
```

Tambien se puede usar:

```powershell
.\i@n-agent.bat --run "prepara comando mikrotik para ver interfaces"
```

## Que entiende esta version

- Web, pagina, HTML, sitio.
- DNS, dominio.
- Puerto, HTTP, HTTPS, servidor.
- Mikrotik, RouterOS, interfaces, IP address.
- Nexo con Python u otros lenguajes.
- Planes estructurados para IA mediante `agente`.

## Vision

La arquitectura queda separada:

- `ian.exe`: motor del lenguaje i@N.
- `ian-agent.exe`: capa inteligente que genera i@N.
- `nexo`: puente para hablar con otros lenguajes o modelos externos.

En una version futura, `ian-agent.exe` puede conectarse a un modelo local o a una
API de IA. La salida seguiria siendo `.ian`, para que el resultado sea auditable,
editable y ejecutable.
