# i@N Daily Learn

`ian-daily.exe` hace que i@N aprenda algo nuevo y lo guarde en memoria local.

## Uso manual

```powershell
ian-daily
ian-daily "redes mikrotik seguridad"
```

## Aprendizaje diario automatico

Se instala una tarea de Windows llamada `iN_Daily_Learn`.

```powershell
schtasks /Create /TN "iN_Daily_Learn" /TR "\"C:\Users\CONECTIA BA\Desktop\ian-lang\ian-daily.cmd\"" /SC DAILY /ST 09:00 /F
```

## Google

i@N no debe scrapear Google directamente. Si queres que use Google de forma
correcta, configura Google Custom Search:

```powershell
setx GOOGLE_API_KEY "tu_api_key"
setx GOOGLE_CX "tu_search_engine_id"
```

Sin esas claves, usa Wikipedia como fuente abierta diaria.

Los aprendizajes quedan en:

- `daily-learning`
- `brain`
- `teach-inbox\aprendido`
