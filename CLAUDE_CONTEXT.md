# Contexto para Claude — i@N Lang

**Última sesión:** 2026-07-29  
**Estado:** v0.5.0 — migración a binario único `ian.exe` con despacho por módulo

---

## Qué es esto

`ian-lang` es un lenguaje de programación en español + una IA local.  
Gabriel lo está construyendo junto a Claude y Codex.  
Motor: C# compilado con .NET 8.0 (SDK-style project).

---

## Arquitectura actual

**Binario único:** `ian.exe` (150 KB, .NET 8.0 publicado como trimmed single-file)

| Módulo | Comando | Fuente | Rol |
|--------|---------|--------|-----|
| engine | `ian.exe <archivo.ian>` | `ian_engine.cs` | Interpreta archivos `.ian` |
| brain | `ian.exe brain ...` | `ian_brain_next.cs` | Cerebro local — memoria + generación de programas |
| shell | `ian.exe shell` | `ian_shell.cs` | Consola interactiva (`i@n` en terminal) |
| human | `ian.exe human ...` | `ian_human.cs` | Capa conversacional + fallback Claude |
| agent | `ian.exe agent ...` | `ian_agent.cs` | Convierte texto a programas i@N |
| teacher | `ian.exe teacher ...` | `ian_teacher.cs` | Importa enseñanzas de Claude/Codex |
| auto | `ian.exe auto ...` | `ian_auto.cs` | Autonomía local |
| api | `ian.exe api ...` | `ian_api.cs` | Conexión API externa |
| daily | `ian.exe daily ...` | `ian_daily.cs` | Aprendizaje automático diario |
| supervisor | `ian.exe supervisor ...` | `ian_supervisor.cs` | Revisor de calidad y seguridad |
| architect | `ian.exe architect ...` | `ian_architect.cs` | Mapa interno para evolucionar i@N |

---

## Compilar

```powershell
cd "C:\Users\CONECTIA BA\Desktop\ian-lang"
.\build.ps1
# o: dotnet build
# Publicar: dotnet publish -c Release -r win-x64 --self-contained
```

---

## Flujo de una conversación en la shell

```
i@n
  → ian.exe shell
     → input normal → ian.exe brain ejecutar "input"
          → BestMemory() busca en brain/memory.tsv
               → si score >= 2 AND cobertura >= 40%: ejecuta el programa memorizado
               → si no: Think() genera un programa desde patrones hardcodeados
                    → ian.exe ejecuta ese programa
     → "hacer <tarea>" → ian.exe agent --run
     → "experto <area>" → ian.exe human perfil
```

---

## brain/memory.tsv

Formato: `prompt_escapado\ttodigo_ian_escapado\ttimestamp\tteacher`  
Escape: `\` → `\\`, tab → `\t`, CR → `\r`, LF → `\n`  
Actualmente: **~180 entradas**

Agregar entrada manualmente:
```powershell
.\ian.exe brain aprender "el prompt" "ia decir \"la respuesta\""
```

---

## Comando `ia` en ian_engine.cs

```ian
ia decir "texto"          # imprime:   i@N >> texto
ia texto "texto"           # imprime:          texto  (continuación)
ia preguntar "pregunta" como variable
ia pensar                  # animación 600ms
ia limpiar                 # limpia pantalla
```

---

## Cambios en v0.5.0 (migración a binario único)

### Qué cambió
- 11 ejecutables separados se unificaron en un solo `ian.exe`
- Cada módulo expone `public static int Run(string[] args)` en vez de `Main()`
- `Program.cs` despacha por primer argumento: `ian.exe brain`, `ian.exe shell`, etc.
- 15 archivos `.exe` individuales eliminados

### Backward compat
- 12 wrappers `.cmd` redirigen (`ian-brain.cmd` → `ian.exe brain %*`)
- `ian_teacher.cs` L298 busca los 3 formatos: `ian.exe brain`, `ian-brain.exe`, `ian-brain`
- Todos los `Process.Start` internos apuntan a `ian.exe <módulo>`

---

## Cómo probar que todo funciona

```powershell
cd "C:\Users\CONECTIA BA\Desktop\ian-lang"

# Test directo del brain:
.\ian.exe brain ejecutar "hola, sos una ia?"
.\ian.exe brain ejecutar "quien te creo?"
.\ian.exe brain ejecutar "que sabes de mi?"

# Test completo via shell:
echo "hola, sos una ia?" | .\ian.exe shell

# Sesión interactiva:
i@n

# Test de módulos:
.\ian.exe brain --version
.\ian.exe shell --version
.\ian.exe agent --version
```

---

## Pendiente / próximas sesiones

- [ ] Funciones definidas por el usuario en i@N (`funcion ... fin`)
- [ ] Plugin RouterOS real (SSH nativa para Mikrotik, no solo texto)
- [ ] Mejorar `AskClaude()` en `ian_human.cs` (llama `claude -p` como proceso externo)
- [ ] Transpilador a JavaScript
- [ ] Más memorias técnicas en `brain/memory.tsv` (subnetting, VPN, firewall específico)
- [ ] Validar flujo completo brain learn → teacher import → agent run
