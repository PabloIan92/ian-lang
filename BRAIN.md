# i@N Brain

`i@N Brain` es el inicio de una inteligencia local propia para i@N.

No usa APIs externas ni modelos de otras empresas. Aprende guardando ejemplos,
memorias y conocimiento local en la carpeta `brain`.

## Importante

i@N Brain no puede copiar el modelo interno de Codex o Claude. Eso no esta
disponible como archivo legible ni seria correcto extraerlo. Lo que si puede
hacer es aprender de:

- conversaciones exportadas por vos,
- ejemplos de codigo,
- correcciones que le enseñes,
- archivos `.txt`, `.md` y `.ian`,
- decisiones que vayas aprobando.

## Uso

```powershell
cd "C:\Users\CONECTIA BA\Desktop\ian-lang"
.\ian-brain.exe estado
.\ian-brain.exe preguntar "crea una web simple"
.\ian-brain.exe crear "prepara comando mikrotik"
.\ian-brain.exe chat
```

## Enseñarle

```powershell
.\ian-brain.exe aprender "crear saludo" "decir \"Hola aprendido por i@N Brain\""
.\ian-brain.exe preguntar "quiero crear saludo"
```

Para evitar problemas con comillas largas, tambien puede aprender desde un
archivo `.ian`:

```powershell
.\ian-brain.exe aprender-archivo "crear saludo" ".\examples\aprender-saludo.ian"
.\ian-brain.exe preguntar "quiero crear saludo"
```

## Importar conocimiento local

Si tenes exportaciones o notas de Codex/Claude en una carpeta:

```powershell
.\ian-brain.exe importar "C:\ruta\a\mis\notas"
```

Eso no copia una IA externa: solo guarda texto local para que i@N Brain tenga
material propio que puedas revisar.
