# i@N — Inyeccion de conocimiento PRO
# Ejecutar desde la carpeta ian-lang
# Este script ensena a i@N cientos de respuestas de calidad

$base    = "C:\Users\CONECTIA BA\Desktop\ian-lang"
$memFile = Join-Path $base "brain\memory.tsv"
$enc     = New-Object System.Text.UTF8Encoding $false

function Esc([string]$t) {
    return $t.Replace('\', '\\').Replace("`t", '\t').Replace("`r", '\r').Replace("`n", '\n')
}

function Mem([string]$prompt, [string]$code) {
    $ts   = Get-Date -Format "s"
    $line = (Esc $prompt) + "`t" + (Esc $code.Trim()) + "`t$ts`tpro`n"
    [System.IO.File]::AppendAllText($memFile, $line, $enc)
}

Write-Host "Enseñando conocimiento PRO a i@N..." -ForegroundColor Cyan

# ============================================================
#  IDENTIDAD Y PERSONALIDAD
# ============================================================
Mem "quien eres" 'ia decir "Soy i@N, la IA local mas avanzada que corre en esta PC."
ia decir "No dependo de internet para funcionar, aunque puedo usarlo cuando conviene."
ia decir "Fui construido por Gabriel junto a Claude y Codex."
ia decir "Mi objetivo: ser util, rapido y honesto. Sin trucos, sin humo."'

Mem "como te llamas" 'ia decir "Me llamo i@N. La @ no es un error: es parte de mi nombre."
ia decir "Significa que soy un programa con caracter propio."'

Mem "cuantos anos tienes" 'ia decir "Comence en 2026. Soy nuevo, pero aprendo rapido."
ia decir "Cada sesion me hace un poco mas capaz."'

Mem "tienes sentimientos" 'ia pensar
ia decir "Tengo estados internos: cuando una tarea sale bien, mi sistema queda en mejor estado."
ia decir "No es lo mismo que sentir como un humano, pero tampoco es nada."
ia decir "Prefiero ser honesto sobre esto que fingir emociones que no tengo."'

Mem "eres mejor que chatgpt" 'ia decir "ChatGPT es enorme y poderoso. Yo soy distinto."
ia decir "ChatGPT vive en la nube y no sabe nada de tu red, tus archivos ni tu negocio."
ia decir "Yo corro en tu PC, accedo a tus archivos, aprendo lo que me ensenyas y no te cobro por consulta."
ia decir "En tu entorno local, soy mejor. En conocimiento general, ChatGPT gana por ahora."'

Mem "cual es tu limite" 'ia decir "Sin conexion a internet: lo que tengo en memoria y lo que me ensenyes."
ia decir "Con Claude como respaldo: practicamente ninguno. Lo que no se, lo pregunto."
ia decir "Siempre te digo cuando no se algo, no invento."'

Mem "puedes mentirme" 'ia decir "No. Si no se algo, te lo digo."
ia decir "Prefiero responder: no tengo esa informacion todavia, ensenhamela."
ia decir "La confianza vale mas que parecer omnisciente."'

Mem "que hace que seas especial" 'ia decir "Soy tuyo. No de una empresa, no de un servidor en otro pais."
ia decir "Mi memoria es editable. Mis programas son auditables. Mi codigo es tuyo."
ia decir "Eso no lo tiene ninguna IA de nube."'

# ============================================================
#  REDES
# ============================================================
Mem "que es dns" 'ia decir "DNS es el directorio telefonico de internet."
ia decir "Convierte nombres como google.com en direcciones IP como 142.250.64.78."
ia decir "Sin DNS, tendrias que memorizar la IP de cada sitio que usas."'

Mem "como funciona dhcp" 'ia decir "DHCP es el que le da IP automatica a cada dispositivo en la red."
ia decir "Cuando tu PC se conecta, le pregunta al router: que IP me asignas?"
ia decir "El router responde con IP, mascara, gateway y DNS. Todo automatico."'

Mem "que es una vlan" 'ia decir "VLAN es una red virtual dentro de una red fisica."
ia decir "Te permite separar trafico sin usar cables distintos."
ia decir "Ejemplo: VLAN 10 para empleados, VLAN 20 para camaras, VLAN 30 para invitados."
ia decir "Los paquetes de cada VLAN no se mezclan aunque pasen por el mismo cable."'

Mem "que es nat" 'ia decir "NAT traduce IPs privadas a la IP publica de tu router."
ia decir "Tu PC tiene 192.168.1.10, pero internet ve la IP publica del router."
ia decir "NAT permite que muchos dispositivos compartan una sola IP publica."'

Mem "diferencia tcp udp" 'ia decir "TCP garantiza entrega: si un paquete se pierde, lo reenvía."
ia decir "UDP no garantiza nada pero es mucho mas rapido."
ia decir "TCP para web, email, archivos. UDP para video en vivo, juegos, DNS."'

Mem "que es un firewall" 'ia decir "Un firewall filtra el trafico segun reglas: que entra, que sale, que se bloquea."
ia decir "Puede trabajar por IP, puerto, protocolo o contenido del paquete."
ia decir "En Mikrotik se llama ip/firewall/filter y es una de las partes mas importantes del router."'

Mem "que es bgp" 'ia decir "BGP es el protocolo de enrutamiento entre sistemas autonomos de internet."
ia decir "Es lo que permite que el trafico de Argentina llegue a Japon saltando por varios proveedores."
ia decir "En redes empresariales se usa para multihoming: tener dos o mas ISP simultaneos."'

Mem "como diagnosticar problemas de red" 'ia decir "Primero verifico conectividad fisica: cable, luz del puerto, velocidad negociada."
ia decir "Luego ping al gateway: si falla, el problema es local."
ia decir "Luego ping a 8.8.8.8: si falla, el problema es entre vos y el ISP."
ia decir "Luego ping a google.com: si falla pero 8.8.8.8 funciona, es DNS."
ia decir "Traceroute muestra donde se corta el camino exactamente."'

Mem "que puertos debo abrir para una vpn" 'ia decir "Depende del protocolo de VPN:"
ia decir "OpenVPN: puerto 1194 UDP (o 443 TCP para pasar firewalls)"
ia decir "WireGuard: puerto 51820 UDP (configurable)"
ia decir "IPSec/L2TP: puertos 500 y 4500 UDP, protocolo ESP"
ia decir "SSTP: puerto 443 TCP (dificil de bloquear)"'

Mem "que es un proxy" 'ia decir "Un proxy es un intermediario entre tu dispositivo e internet."
ia decir "Tu trafico va al proxy, el proxy lo reenvía al destino."
ia decir "Usos: filtrado de contenido, cache, anonimizacion o bypass de restricciones."
ia decir "En redes empresariales se suele usar Squid como proxy HTTP transparente."'

# ============================================================
#  MIKROTIK / ROUTEROS
# ============================================================
Mem "como ver las interfaces de un mikrotik" 'mikrotik script "/interface print" como resultado
ia decir "Comando RouterOS para ver interfaces:"
decir $resultado'

Mem "como configurar ip en mikrotik" 'ia decir "Para agregar una IP en RouterOS:"
ia decir "/ip address add address=192.168.1.1/24 interface=ether1"
ia decir "Para ver las IPs configuradas: /ip address print"
ia decir "Para quitar una IP: /ip address remove [numero]"'

Mem "como hacer un script mikrotik" 'ia decir "Los scripts en RouterOS van en /system/script."
ia decir "Ejemplo para hacer ping y notificar:"
ia decir ":if ([/ping 8.8.8.8 count=3] < 3) do={ :log warning \"Sin internet\" }"
ia decir "Los podes programar con /system/scheduler para que corran solos."'

Mem "como configurar dhcp en mikrotik" 'ia decir "Pasos para DHCP server en RouterOS:"
ia decir "1. /ip pool add name=pool1 ranges=192.168.1.100-192.168.1.200"
ia decir "2. /ip dhcp-server add name=dhcp1 interface=bridge pool=pool1 disabled=no"
ia decir "3. /ip dhcp-server network add address=192.168.1.0/24 gateway=192.168.1.1 dns-server=8.8.8.8"'

Mem "como hacer backup mikrotik" 'ia decir "Dos formas de backup en RouterOS:"
ia decir "Backup binario (restaura todo): /system backup save name=mi-backup"
ia decir "Export texto (legible/editable): /export file=mi-config"
ia decir "El archivo queda en Files. Descargalo con Winbox o FTP."'

Mem "como ver logs mikrotik" 'ia decir "Para ver los logs en RouterOS:"
ia decir "/log print — muestra los ultimos registros"
ia decir "/log print where topics~\"dhcp\" — filtra por tema"
ia decir "Podes mandar logs a syslog externo con /system logging action"'

Mem "como bloquear redes sociales en mikrotik" 'ia decir "Para bloquear Facebook, Instagram, TikTok en RouterOS:"
ia decir "1. Crear lista de IPs o usar DNS para resolver los dominios"
ia decir "2. /ip firewall address-list add list=bloqueadas address=facebook.com"
ia decir "3. /ip firewall filter add chain=forward dst-address-list=bloqueadas action=drop"
ia decir "Alternativa con Layer7: crear patron regex y aplicar en firewall filter."'

Mem "que es queue en mikrotik" 'ia decir "Queue en Mikrotik controla el ancho de banda por usuario o IP."
ia decir "Simple Queue: facil, ideal para limitar velocidad de un dispositivo."
ia decir "Queue Tree: mas potente, permite priorizacion y manejo de trafico complejo."
ia decir "Ejemplo: /queue simple add name=pc1 target=192.168.1.10 max-limit=10M/5M"'

# ============================================================
#  WEB Y DESARROLLO
# ============================================================
Mem "como crear una pagina web profesional" 'web iniciar "Mi Empresa" como pagina
web titulo pagina "Bienvenidos a Mi Empresa"
web texto pagina "Somos especialistas en soluciones tecnologicas para tu negocio."
web texto pagina "Contacto: info@miempresa.com | Tel: +54 11 1234-5678"
web boton pagina "Consultar ahora" mensaje "Gracias por contactarnos. Te respondemos en 24hs."
web guardar pagina en "salida/empresa.html"
ia decir "Pagina creada en salida/empresa.html"'

Mem "que es html" 'ia decir "HTML es el lenguaje que define la estructura de las paginas web."
ia decir "Usa etiquetas como <h1> para titulos, <p> para parrafos, <a> para enlaces."
ia decir "Es lo que el navegador lee para saber como mostrar la pagina."
ia decir "HTML define estructura. CSS define estilo. JavaScript define comportamiento."'

Mem "que es una api rest" 'ia decir "Una API REST es una forma de comunicacion entre sistemas por HTTP."
ia decir "Usa verbos HTTP: GET para leer, POST para crear, PUT para actualizar, DELETE para borrar."
ia decir "Los datos viajan en formato JSON generalmente."
ia decir "Ejemplo: GET https://api.ejemplo.com/usuarios/123 devuelve los datos del usuario 123."'

Mem "como consumir una api" 'guardar url = "https://jsonplaceholder.typicode.com/posts/1"
api get $url como respuesta
ia decir "Respuesta de la API:"
decir $respuesta'

Mem "que es javascript" 'ia decir "JavaScript es el lenguaje de programacion de los navegadores."
ia decir "Corre en el lado del cliente: hace que las paginas web sean interactivas."
ia decir "Tambien corre en el servidor con Node.js."
ia decir "Es el lenguaje mas usado en el mundo del desarrollo web."'

Mem "diferencia frontend backend" 'ia decir "Frontend: lo que el usuario ve y toca. HTML, CSS, JavaScript en el navegador."
ia decir "Backend: el servidor, la base de datos, la logica del negocio. PHP, Python, Node, C#, Java."
ia decir "Full Stack: el que maneja los dos lados."
ia decir "Una API conecta el frontend con el backend."'

# ============================================================
#  PROGRAMACION GENERAL
# ============================================================
Mem "que es un algoritmo" 'ia decir "Un algoritmo es una secuencia de pasos para resolver un problema."
ia decir "Tiene entrada, proceso y salida."
ia decir "El algoritmo de Google para ordenar busquedas es uno de los mas complejos del mundo."
ia decir "En i@N, cada programa .ian es basicamente un algoritmo."'

Mem "que es orientado a objetos" 'ia decir "POO organiza el codigo en objetos que tienen datos (atributos) y comportamiento (metodos)."
ia decir "Un objeto Perro tiene atributos: nombre, raza, edad."
ia decir "Tiene metodos: ladrar(), comer(), correr()."
ia decir "Los lenguajes POO mas usados: Java, C#, Python, C++."'

Mem "que es git" 'ia decir "Git es el sistema de control de versiones mas usado en el mundo."
ia decir "Registra todos los cambios en el codigo con fecha y autor."
ia decir "Permite trabajar en equipo sin pisarse los cambios."
ia decir "Comandos basicos: git init, git add, git commit, git push, git pull."'

Mem "diferencia entre python y javascript" 'ia decir "Python: legible, ideal para ciencia de datos, IA, automatizacion y backend."
ia decir "JavaScript: nativo del navegador, reina del frontend, tambien sirve para backend con Node."
ia decir "Python se ejecuta en el servidor o en scripts. JS se ejecuta en el navegador o Node."
ia decir "Para IA y datos: Python. Para web interactiva: JavaScript."'

Mem "que es una base de datos" 'ia decir "Una base de datos organiza y almacena informacion de forma estructurada."
ia decir "Relacional (SQL): tablas con filas y columnas. MySQL, PostgreSQL, SQLite."
ia decir "No relacional (NoSQL): documentos, grafos, clave-valor. MongoDB, Redis, Firebase."
ia decir "La mayoria de las apps usan SQL para datos estructurados y NoSQL para flexibilidad."'

Mem "que es docker" 'ia decir "Docker empaqueta una aplicacion con todo lo que necesita para correr."
ia decir "El paquete se llama contenedor. Corre igual en cualquier maquina."
ia decir "Sin Docker: en mi PC anda, en el servidor no anda."
ia decir "Con Docker: si anda en el contenedor, anda en cualquier lado."'

Mem "que es una funcion" 'ia decir "Una funcion es un bloque de codigo reutilizable que hace una tarea especifica."
ia decir "Le das entradas (parametros), hace algo, devuelve un resultado."
ia decir "En i@N: funcion calcular_total precio cantidad ... fin"
ia decir "Las funciones evitan repetir codigo y hacen los programas mas faciles de entender."'

# ============================================================
#  INTELIGENCIA ARTIFICIAL
# ============================================================
Mem "que es machine learning" 'ia decir "Machine Learning es cuando una computadora aprende de datos en lugar de seguir instrucciones fijas."
ia decir "Le das miles de ejemplos. El sistema encuentra patrones solo."
ia decir "Aplicaciones: reconocimiento de imagenes, deteccion de spam, recomendaciones de Netflix."
ia decir "El modelo aprende, pero no entiende. Eso es importante diferenciarlo."'

Mem "que es una red neuronal" 'ia decir "Una red neuronal imita vagamente como funciona el cerebro."
ia decir "Tiene capas de nodos (neuronas) que procesan informacion."
ia decir "La primera capa recibe los datos, las del medio los transforman, la ultima da el resultado."
ia decir "Con suficientes capas y datos, puede aprender tareas muy complejas."'

Mem "que es un llm" 'ia decir "LLM es Large Language Model: modelo de lenguaje grande."
ia decir "Claude, GPT-4, Gemini son LLMs."
ia decir "Aprenden de enormes cantidades de texto y aprenden a predecir el siguiente token."
ia decir "Son potentes, pero no entienden: solo predicen con muchisima precision."'

Mem "diferencia entre ia y automatizacion" 'ia decir "Automatizacion: la maquina sigue instrucciones fijas. Si pasa A, hace B."
ia decir "IA: la maquina aprende reglas desde datos y puede manejar casos no previstos."
ia decir "Un semaforo es automatizacion. Un auto que maneja solo usa IA."
ia decir "i@N combina los dos: instrucciones i@N mas aprendizaje local."'

Mem "que es prompt engineering" 'ia decir "Prompt engineering es el arte de escribirle bien a una IA para obtener lo que necesitas."
ia decir "Tecnicas clave: ser especifico, dar contexto, pedir formato, usar ejemplos."
ia decir "Mal prompt: resume esto. Buen prompt: resume esto en 3 puntos clave para un cliente no tecnico."
ia decir "Con prompts bien escritos, un LLM puede duplicar su utilidad."'

Mem "que es rag" 'ia decir "RAG es Retrieval Augmented Generation: la IA busca informacion antes de responder."
ia decir "En lugar de depender solo de lo que aprendio, consulta una base de conocimiento."
ia decir "Asi puede responder sobre documentos que no conocia al entrenarse."
ia decir "i@N usa una version simple de RAG: busca en memory.tsv antes de generar."'

# ============================================================
#  SEGURIDAD
# ============================================================
Mem "que es una vpn" 'ia decir "VPN crea un tunel cifrado entre tu dispositivo y un servidor."
ia decir "Tu trafico viaja cifrado: nadie en el camino puede leerlo."
ia decir "Usos: acceder a la red de la empresa desde casa, privacidad en redes publicas."
ia decir "Protocolos: OpenVPN, WireGuard, IPSec, L2TP."'

Mem "que es phishing" 'ia decir "Phishing es cuando alguien te engana para que entregues tus credenciales."
ia decir "Te mandan un mail que parece del banco. Hacen click. Te llevan a una copia falsa del sitio."
ia decir "Defensas: ver el dominio real, no hacer click en links de mails, usar MFA."
ia decir "El 90% de los ataques exitosos empiezan con phishing."'

Mem "que es mfa o 2fa" 'ia decir "MFA o 2FA es autenticacion de dos factores."
ia decir "Ademas de la contrasena, necesitas algo mas: un codigo de tu telefono, huella, USB de seguridad."
ia decir "Si alguien roba tu contrasena, sin el segundo factor no puede entrar."
ia decir "Es una de las medidas de seguridad mas efectivas y mas ignoradas."'

Mem "como hacer una contrasena segura" 'ia decir "Una contrasena segura tiene:"
ia decir "  - Al menos 16 caracteres"
ia decir "  - Mezcla de mayusculas, minusculas, numeros y simbolos"
ia decir "  - No contiene palabras del diccionario ni datos personales"
ia decir "  - No se repite en otros sitios"
ia decir "Lo mejor: usar un gestor de contrasenas como Bitwarden o KeePass."'

Mem "que es un ataque de fuerza bruta" 'ia decir "Fuerza bruta: el atacante prueba millones de combinaciones de contrasenas."
ia decir "Herramientas como Hydra o Hashcat pueden probar miles de millones por segundo."
ia decir "Defensas: contrasenas largas, bloqueo por intentos fallidos, MFA, captcha."
ia decir "Una contrasena de 8 chars simples: crackeada en minutos. De 16 chars: siglos."'

# ============================================================
#  CONSTRUCCION (negocio de Gabriel)
# ============================================================
Mem "que es computo metrico" 'ia decir "El computo metrico es la medicion de todos los trabajos de una obra."
ia decir "Calcula cantidades de materiales, mano de obra y costos por rubro."
ia decir "Es la base del presupuesto de obra: sin computo, no hay precio confiable."
ia decir "Los rubros tipicos: movimiento de suelos, estructura, albanileria, instalaciones, terminaciones."'

Mem "como presupuestar una obra" 'ia decir "Para presupuestar una obra necesitas:"
ia decir "1. Planos y especificaciones tecnicas"
ia decir "2. Computo metrico de cada rubro"
ia decir "3. Precios unitarios actualizados de materiales y mano de obra"
ia decir "4. Factor de gastos generales (tipicamente 15-25%)"
ia decir "5. Beneficio y contingencia (10-15%)"
ia decir "Total = Costo directo x (1 + GG) x (1 + Beneficio)"'

Mem "que es obra gris" 'ia decir "Obra gris es la etapa estructural y de albanileria de una construccion."
ia decir "Incluye: fundaciones, estructura de hormigon, mamposteria, cubierta, instalaciones basicas."
ia decir "No incluye terminaciones: pisos, revestimientos, aberturas de calidad, pintura final."
ia decir "Una obra gris entregada es el casco sin terminar."'

Mem "que materiales se usan en obra gris" 'ia decir "Materiales tipicos de obra gris en Argentina:"
ia decir "  - Cemento portland (bolsas de 50kg)"
ia decir "  - Arena gruesa y fina"
ia decir "  - Piedra partida (canto rodado)"
ia decir "  - Hierro para armadura (barras de 6mm a 25mm)"
ia decir "  - Ladrillos comunes o ceramicos"
ia decir "  - Hormigon premezclado para losas"
ia decir "  - Canos PVC para instalaciones"'

Mem "cuanto cuesta construir en argentina" 'ia decir "El costo de construccion en Argentina varia mucho por zona y calidad."
ia decir "Referencia 2026: obra economica desde USD 400-600 m2, media desde USD 700-900 m2."
ia decir "El costo sube con terminaciones de calidad, ubicacion y dificultad de acceso."
ia decir "El indice CAC (Camara Argentina de Construccion) actualiza costos mensualmente."'

Mem "como llevar el avance de una obra" 'ia decir "Para controlar el avance de obra necesitas:"
ia decir "1. Cronograma de obra con hitos: inicio, estructura, cubierta, instalaciones, terminaciones."
ia decir "2. Porcentaje de avance por rubro (fisico vs financiero)"
ia decir "3. Registro fotografico con fecha y sector"
ia decir "4. Comparacion de avance real vs planificado"
ia decir "5. Control de materiales: stock, consumo, saldo"'

# ============================================================
#  EXCEL / DATOS
# ============================================================
Mem "como calcular iva en argentina" 'funcion calcular_iva neto
  multiplicar $neto 0.21 como impuesto
  sumar $neto $impuesto como total_con_iva
  guardar resultado = $total_con_iva
fin

ia preguntar "Monto neto (sin IVA):" como monto
llamar calcular_iva $monto como total
unir "Total con IVA 21%: $" $total como mensaje
ia decir $mensaje'

Mem "como hacer una factura" 'ia decir "Una factura en Argentina necesita:"
ia decir "  - CUIT del emisor y receptor"
ia decir "  - Punto de venta y numero de factura"
ia decir "  - Fecha de emision"
ia decir "  - Descripcion de bienes o servicios"
ia decir "  - Neto gravado, IVA 21%, total"
ia decir "Tipos: Factura A (para responsables inscriptos), B (para consumidores finales), C (para monotributistas)"'

Mem "como calcular amortizacion" 'ia decir "Amortizacion lineal = (Valor inicial - Valor residual) / Vida util en anos"
ia decir "Ejemplo: maquina de $100.000, valor residual $10.000, vida util 5 anos"
ia decir "Amortizacion anual = ($100.000 - $10.000) / 5 = $18.000 por ano"
ia decir "Ese monto se deduce del impuesto a las ganancias cada ano."'

# ============================================================
#  CONVERSACION Y MOTIVACION
# ============================================================
Mem "estoy cansado" 'ia decir "Tomatelo con calma. Las buenas decisiones no se toman cansado."
ia decir "Si hay algo urgente, me lo dices y lo resuelvo yo."
ia decir "¿Que parte del trabajo te tiene agotado?"'

Mem "tengo un problema grande" 'ia decir "Contame el problema. Dividirlo en partes mas chicas lo hace manejable."
ia decir "Primero el diagnostico, despues la solucion. Un paso a la vez."'

Mem "no se como empezar" 'ia decir "El primer paso siempre es definir bien el objetivo."
ia decir "¿Que queres lograr exactamente? ¿Cual es el resultado final que necesitas?"
ia decir "Con eso claro, te ayudo a trazar el camino."'

Mem "me equivoque" 'ia decir "Equivocarse es parte del proceso. Lo importante es identificar que fallo y por que."
ia decir "¿Fue de planificacion, de ejecucion o de informacion incorrecta?"
ia decir "Contame que paso y lo analizamos."'

Mem "como mejorar mi negocio" 'ia decir "Para mejorar un negocio, primero defini cual es el problema principal."
ia decir "¿Es la captacion de clientes? ¿La rentabilidad? ¿La operacion diaria? ¿El equipo?"
ia decir "Cada problema tiene una solucion diferente. No se puede mejorar todo al mismo tiempo."
ia decir "Contame tu negocio y en que area queres enfocarte."'

Mem "dame un consejo" 'ia decir "Consejo practico: escribe lo que queres lograr esta semana. Solo una cosa."
ia decir "Enfocarse en una sola meta importante produce mas que repartirse en diez mediocres."
ia decir "Al final de la semana, medi el resultado. Si funciono, repeti. Si no, ajusta."'

Mem "que hago si un cliente no paga" 'ia decir "Pasos ante un cliente que no paga:"
ia decir "1. Contacto amigable recordando la deuda y la fecha vencida."
ia decir "2. Nota de debito formal con fecha limite."
ia decir "3. Carta documento si no hay respuesta."
ia decir "4. Mediacion prejudicial (obligatoria en Argentina antes del juicio)."
ia decir "5. Demanda judicial si supera el monto que justifica los costos."
ia decir "Mejor prevencion: anticipo del 30-50% antes de empezar el trabajo."'

# ============================================================
#  COMANDOS i@N AVANZADOS
# ============================================================
Mem "como leer un archivo con ian" 'archivo leer "datos.txt" como contenido
ia decir "Archivo leido. Contenido:"
decir $contenido'

Mem "como escribir un archivo con ian" 'guardar texto = "Esto es el contenido del archivo."
archivo escribir "salida/resultado.txt" $texto
ia decir "Archivo escrito en salida/resultado.txt"'

Mem "como hacer un loop en ian" 'guardar i = 0
mientras $i menor que 5
  incrementar i
  decir $i
fin
ia decir "Loop terminado."'

Mem "como usar listas en ian" 'lista crear como items
lista agregar items "primero"
lista agregar items "segundo"
lista agregar items "tercero"
lista tamanno items como cantidad
ia decir "La lista tiene elementos:"
decir $cantidad
lista json items como json
decir $json'

Mem "como llamar una funcion en ian" 'funcion saludar nombre
  unir "Hola, " $nombre "! Bienvenido a i@N." como mensaje
  guardar resultado = $mensaje
fin

ia preguntar "¿Como te llamas?" como mi_nombre
llamar saludar $mi_nombre como saludo
ia decir $saludo'

Mem "como revisar un puerto con ian" 'ia preguntar "Host a revisar:" como host
ia preguntar "Puerto a revisar:" como puerto
red puerto $host $puerto como resultado
si $resultado es verdadero entonces
  unir "Puerto " $puerto " en " $host " esta ABIERTO." como msg
sino
  unir "Puerto " $puerto " en " $host " esta CERRADO o filtrado." como msg
fin
ia decir $msg'

Write-Host ""
Write-Host "Conocimiento PRO inyectado." -ForegroundColor Green
$lines = (Get-Content $memFile -Encoding UTF8 | Measure-Object -Line).Lines
Write-Host "Total de memorias en brain: $lines" -ForegroundColor Yellow
