# Especificación i@N 0.4.3

## Filosofía

i@N prioriza lectura humana, automatización cotidiana y conexión entre sistemas.
La idea no es aislarse de otros lenguajes, sino coordinarlos con una sintaxis   
más directa para tareas técnicas.

## Reglas

- Un comando por línea.
- Comentarios con `#`.
- Las cadenas van entre comillas.
- Las variables se guardan con `guardar nombre = valor`.
- Las variables se leen con `$nombre`.
- Los bloques terminan con `fin`.

## Novedades v0.4.3

### Funciones Propias

Permite modularizar el código. El valor de retorno se define mediante la variable `resultado` dentro de la función.

```ian
funcion calcular_doble n
  multiplicar $n 2 como resultado
fin

llamar calcular_doble 10 como mi_doble
decir "El doble de 10 es" $mi_doble
```

### Repetición para cada (Foreach)

Permite recorrer los elementos de una lista.

```ian
lista crear como colores
lista agregar colores "Rojo"
lista agregar colores "Azul"

repetir para cada c en colores
  decir "Color encontrado:" $c
fin
```

### Mejora en `decir`

Ahora acepta múltiples argumentos que se concatenan con un espacio.

```ian
guardar nombre = "Ian"
decir "Hola," $nombre ". Bienvenido."
```

### Condicionales de Archivo Directos

Permite comprobar la existencia de un archivo directamente en un bloque `si`.

```ian
si archivo existe "config.txt" entonces
  decir "Cargando configuracion..."
sino
  decir "Archivo no encontrado."
fin
```

---

## Comandos Estándar

### Módulos
`incluir "archivo.ian"`: Ejecuta otro archivo en el contexto actual.

### Archivos
- `archivo leer "ruta" como variable`
- `archivo escribir "ruta" contenido`
- `archivo agregar "ruta" contenido`
- `archivo existe "ruta" como variable`

### Salida
`decir valor1 valor2 ...`: Muestra texto en la consola.

### Variables
`guardar nombre = valor`: Define una variable.

### Repetición
- `repetir N veces ... fin`
- `repetir para cada item en lista ... fin`

### Red
- `red dns "host" como variable`
- `red puerto "host" puerto como variable`
- `red ping "host" como variable` (requiere --allow-run)

### Web
- `web iniciar "Titulo" como pagina`
- `web titulo pagina "Encabezado"`
- `web texto pagina "Contenido"`
- `web boton pagina "Etiqueta" mensaje "Alerta"`
- `web guardar pagina en "ruta.html"`

### Nexo
`nexo nombre = proceso "comando"`: Define un puente externo.
`llamar nexo con "dato" como variable`: Invoca el nexo.

### API
`api get "url" como variable`: Solicitud HTTP GET simple.

### Agente IA
Estructuras para planes de IA: `agente plan`, `agente paso`, `agente json`.
