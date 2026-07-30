# i@N para inteligencias artificiales

i@N debe ser facil para humanos y tambien comodo para IA. Eso significa que el
lenguaje necesita reglas simples, salidas estructuradas y pocos caminos ambiguos.

## Principios para IA

- Una accion por linea.
- Comandos con nombres claros en espanol.
- Resultados que puedan ser JSON cuando haga falta.
- Integracion con otros lenguajes por `nexo`, sin obligar a reescribir todo.
- Archivos `.ian` faciles de leer, auditar y corregir.
- Errores concretos: el interprete debe decir que comando fallo y por que.

## Comando agente

`agente` permite crear planes estructurados para que una IA pueda explicar,
ejecutar o pasar tareas a otros sistemas.

```ian
agente plan "Crear una app simple" como plan
agente paso plan hacer "Crear interfaz"
agente paso plan hacer "Conectar API"
agente paso plan hacer "Probar flujo"
agente json plan como salida
decir $salida
```

Esto produce una estructura que otra herramienta puede leer.

## Por que puede ser poderoso

i@N no compite solo por velocidad de ejecucion. Su fuerza esta en funcionar como
idioma comun entre:

- Una persona que describe una tarea.
- Una IA que genera pasos o codigo.
- Herramientas de red como Mikrotik.
- Web, scripts, APIs y programas externos.
- Lenguajes existentes como Python, JavaScript, PowerShell, Java, Go o Rust.

La version actual es pequena, pero la arquitectura correcta es convertirlo en
un lenguaje orquestador: i@N decide, conecta y simplifica; otros runtimes hacen
el trabajo especializado cuando conviene.
