# i@N API

`i@N API` permite conectar i@N con otros sistemas cuando necesita informacion
externa.

## Uso

```powershell
ian-api ejemplo
ian-api agregar wiki "https://es.wikipedia.org/api/rest_v1/page/summary/{pregunta}"
ian-api preguntar wiki "MikroTik"
```

La palabra `{pregunta}` se reemplaza por la consulta codificada.

## Desde i@n

```txt
api listar
api agregar wiki https://es.wikipedia.org/api/rest_v1/page/summary/{pregunta}
api preguntar wiki MikroTik
```

## Sistemas propios

Tambien podes conectar APIs internas:

```powershell
ian-api agregar clientes "https://mi-servidor.local/clientes?q={pregunta}"
ian-api preguntar clientes "cliente 123"
```

## Seguridad

Esta version usa `GET` simple y no guarda claves secretas. Si una API necesita
token, conviene crear un proxy local o una API interna que oculte el secreto.
