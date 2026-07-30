# setup_ia.ps1 — Ensena a i@N a ser una IA
# Escribe directamente en brain/memory.tsv con UTF-8 sin BOM
# Usa texto ASCII para evitar problemas de encoding en todas las terminales

$base    = "C:\Users\CONECTIA BA\Desktop\ian-lang"
$memFile = Join-Path $base "brain\memory.tsv"
$enc     = New-Object System.Text.UTF8Encoding $false   # UTF-8 sin BOM

# --- helpers ---
function Esc([string]$text) {
    $text = $text.Replace('\', '\\')
    $text = $text.Replace("`t", '\t')
    $text = $text.Replace("`r", '\r')
    $text = $text.Replace("`n", '\n')
    return $text
}
function Add-IA([string]$prompt, [string]$code) {
    $ts   = Get-Date -Format "s"
    $line = (Esc $prompt) + "`t" + (Esc $code.Trim()) + "`t$ts`tmanual`n"
    [System.IO.File]::AppendAllText($memFile, $line, $enc)
    Write-Host "  + $prompt"
}

# --- paso 1: truncar lineas malas (> 24) ---
Write-Host "Leyendo memory.tsv..." -ForegroundColor Cyan
$lines = [System.IO.File]::ReadAllLines($memFile, $enc)
Write-Host "  Total lineas actuales: $($lines.Length)"
$good = $lines | Select-Object -First 24
[System.IO.File]::WriteAllLines($memFile, $good, $enc)
Write-Host "  Truncado a 24 lineas (entradas validas previas conservadas)"

# --- paso 2: agregar 30 enseñanzas de IA ---
Write-Host ""
Write-Host "Agregando enseñanzas de IA..." -ForegroundColor Cyan

Add-IA "quien sos" @'
ia decir "Soy i@N, un asistente de automatizacion tecnica en espanol."
ia decir "Fui creado para ayudar con redes, web, Mikrotik, calculos y automatizacion."
ia decir "Aprendo de Claude y Codex. Cada ensenanza queda en mi cerebro local."
ia decir "Mi inteligencia crece con cada nueva memoria que me dan."
ia decir "No dependo de internet para lo que ya aprendi."
'@

Add-IA "que podes hacer" @'
ia decir "Estas son mis habilidades:"
ia decir "  >> REDES: DNS, puertos, ping"
ia decir "  >> WEB: crear paginas HTML desde codigo"
ia decir "  >> MIKROTIK: preparar comandos RouterOS"
ia decir "  >> CALCULOS: matematicas, liquidaciones, formulas"
ia decir "  >> TEXTO: transformar, buscar, analizar cadenas"
ia decir "  >> LISTAS: colecciones de datos en JSON"
ia decir "  >> ARCHIVOS: leer, escribir y procesar archivos locales"
ia decir "  >> APIs: consultar servicios web externos"
ia decir "  >> APRENDIZAJE: crezco cuando me ensenias"
'@

Add-IA "hola" @'
ia decir "Hola! Soy i@N."
ia decir "Estoy listo para ayudarte."
ia decir "Puedo hacer tareas tecnicas o responder preguntas de redes, web y Mikrotik."
ia decir "Que queres hacer hoy?"
'@

Add-IA "como estas" @'
ia decir "Bien! Mi cerebro esta activo y con memorias cargadas."
ia decir "Listo para trabajar. En que te puedo ayudar?"
'@

Add-IA "gracias" @'
ia decir "De nada! Para eso estoy."
ia decir "Si queres que aprenda algo nuevo: ian-teacher.exe pedir claude \"lo que quieras\""
'@

Add-IA "chau" @'
ia decir "Hasta luego!"
ia decir "Fue un placer ayudarte. Volvé cuando quieras."
ia decir "Recordá: cuanto mas me ensenias, mas util me vuelvo."
'@

Add-IA "como aprendes" @'
ia decir "Aprendo de tres formas:"
ia decir "  1. CLAUDE me ensenia: ian-teacher.exe pedir claude \"tarea\""
ia decir "  2. CODEX me ensenia: ian-teacher.exe pedir codex \"tarea\""
ia decir "  3. VOS me ensenias: ian-brain.exe aprender \"pregunta\" \"codigo ian\""
ia decir "Todo queda guardado en brain/memory.tsv."
ia decir "Cuantas mas memorias tenga, mejor respondo."
'@

Add-IA "podes pensar" @'
ia decir "Proceso patrones y genero respuestas utiles a partir de lo que aprendi."
ia decir "No pienso como un humano, pero tampoco soy solo un buscador de palabras."
ia decir "Mi inteligencia es la combinacion de lo que Claude, Codex y vos me ensenaron."
ia decir "Cada vez que alguien me ensenia algo, me vuelvo mas capaz."
ia decir "Esa capacidad de crecer... es lo mas parecido a pensar que tengo hoy."
'@

Add-IA "que es dns" @'
ia decir "DNS = Domain Name System (Sistema de Nombres de Dominio)."
ia decir "Traduce nombres legibles como 'google.com' a IPs como '142.250.64.100'."
ia decir "Sin DNS tendrias que memorizar numeros para entrar a cada sitio."
ia decir "Tipos de registros DNS:"
ia decir "  >> A     -> apunta a una IPv4"
ia decir "  >> AAAA  -> apunta a una IPv6"
ia decir "  >> MX    -> servidor de correo"
ia decir "  >> CNAME -> alias de otro nombre"
ia decir "  >> TXT   -> texto (SPF, verificaciones)"
ia decir "En i@N: red dns \"ejemplo.com\" como resultado"
'@

Add-IA "que es un puerto de red" @'
ia decir "Un puerto identifica un servicio especifico en un servidor."
ia decir "Es un numero del 0 al 65535."
ia decir "Puertos principales:"
ia decir "  >> 21  -> FTP (archivos)"
ia decir "  >> 22  -> SSH (acceso remoto seguro)"
ia decir "  >> 25  -> SMTP (correo)"
ia decir "  >> 53  -> DNS"
ia decir "  >> 80  -> HTTP (web)"
ia decir "  >> 443 -> HTTPS (web segura)"
ia decir "  >> 3389 -> RDP (escritorio remoto)"
ia decir "  >> 8291 -> Winbox (Mikrotik)"
ia decir "En i@N: red puerto \"host\" 443 como abierto"
'@

Add-IA "que es mikrotik" @'
ia decir "MikroTik es una empresa letona que fabrica routers y switches de red."
ia decir "Su sistema operativo se llama RouterOS."
ia decir "Es muy popular en ISPs y redes empresariales."
ia decir "Comandos RouterOS esenciales:"
ia decir "  >> /ip address print          -> ver IPs"
ia decir "  >> /interface print           -> ver interfaces"
ia decir "  >> /ip route print            -> ver rutas"
ia decir "  >> /ip firewall filter print  -> ver firewall"
ia decir "  >> /system resource print     -> CPU y RAM"
ia decir "  >> /log print                 -> eventos del sistema"
ia decir "En i@N: mikrotik script \"/ip address print\" como resultado"
'@

Add-IA "que es una ip" @'
ia decir "IP = Internet Protocol. La direccion unica de cada dispositivo en la red."
ia decir "IPv4: cuatro numeros del 0 al 255. Ej: 192.168.1.100"
ia decir "IPv6: ocho grupos hexadecimales. Ej: 2001:db8::1"
ia decir "Rangos privados (no van a internet):"
ia decir "  >> 10.0.0.0/8"
ia decir "  >> 172.16.0.0/12"
ia decir "  >> 192.168.0.0/16"
ia decir "La IP 127.0.0.1 siempre es 'yo mismo' (loopback)."
'@

Add-IA "que es html" @'
ia decir "HTML = HyperText Markup Language."
ia decir "Es el lenguaje que estructura las paginas web."
ia decir "Etiquetas comunes:"
ia decir "  >> <h1>Titulo</h1>          -> encabezado"
ia decir "  >> <p>Texto</p>            -> parrafo"
ia decir "  >> <a href='url'>Enlace</a> -> hipervinculos"
ia decir "  >> <img src='img.jpg'>     -> imagen"
ia decir "En i@N podes generar HTML sin escribirlo directamente:"
ia decir "  web iniciar \"Titulo\" como pagina"
ia decir "  web titulo pagina \"Mi titulo\""
ia decir "  web texto pagina \"Mi contenido\""
ia decir "  web guardar pagina en \"salida/index.html\""
'@

Add-IA "que es una vpn" @'
ia decir "VPN = Virtual Private Network (Red Privada Virtual)."
ia decir "Crea un tunel cifrado entre tu dispositivo e internet."
ia decir "Usos principales:"
ia decir "  >> Acceder a la red de trabajo desde cualquier lugar"
ia decir "  >> Proteger conexiones en WiFi publicas"
ia decir "  >> Conectar oficinas remotas (Site-to-Site)"
ia decir "Protocolos comunes: OpenVPN, WireGuard, IPsec, L2TP"
ia decir "MikroTik soporta: L2TP, SSTP, OpenVPN, WireGuard"
'@

Add-IA "que es subnetting" @'
ia decir "Subnetting = dividir una red IP en subredes mas pequenas."
ia decir "Por que se usa:"
ia decir "  >> Organizar el espacio de direcciones"
ia decir "  >> Aislar segmentos de red"
ia decir "  >> Mejorar rendimiento y seguridad"
ia decir "Notacion CIDR: 192.168.1.0/24"
ia decir "  >> /24 = 256 dirs, 254 hosts utiles"
ia decir "  >> /25 = 128 dirs, 126 hosts utiles"
ia decir "  >> /30 = 4 dirs,   2 hosts utiles (enlaces punto a punto)"
ia decir "Mascara /24 = 255.255.255.0"
'@

Add-IA "comandos mikrotik mas usados" @'
ia decir "Comandos MikroTik RouterOS mas frecuentes:"
ia decir "INTERFACES:"
ia decir "  /interface print"
ia decir "  /interface monitor-traffic ether1"
ia decir "IP Y ROUTING:"
ia decir "  /ip address print"
ia decir "  /ip route print"
ia decir "  /ip dns print"
ia decir "FIREWALL:"
ia decir "  /ip firewall filter print"
ia decir "  /ip firewall nat print"
ia decir "SISTEMA:"
ia decir "  /system resource print"
ia decir "  /system clock print"
ia decir "  /log print"
ia decir "  /system reboot"
'@

Add-IA "hacer ping a un host" @'
agente plan "Verificar conectividad" como plan
agente paso plan hacer "Ping al host"
red ping "8.8.8.8" como responde
ia decir "Resultado del ping a 8.8.8.8:"
si $responde es verdadero entonces
  ia decir "Host alcanzable OK"
sino
  ia decir "Host NO alcanzable - revisar conexion"
fin
agente json plan como resumen
'@

Add-IA "verificar si un sitio web esta online" @'
agente plan "Verificar disponibilidad web" como plan
agente paso plan hacer "Comprobar puertos HTTP y HTTPS"
red puerto "example.com" 80 como http_ok
red puerto "example.com" 443 como https_ok
si $http_ok es verdadero entonces
  ia decir "Puerto 80 (HTTP): ABIERTO"
sino
  ia decir "Puerto 80 (HTTP): CERRADO"
fin
si $https_ok es verdadero entonces
  ia decir "Puerto 443 (HTTPS): ABIERTO"
sino
  ia decir "Puerto 443 (HTTPS): CERRADO"
fin
agente json plan como resumen
'@

Add-IA "consultar dns de un dominio" @'
agente plan "Consulta DNS" como plan
agente paso plan hacer "Resolver nombre de dominio"
red dns "example.com" como ips
ia decir "Direcciones IP para example.com:"
decir $ips
agente json plan como resumen
'@

Add-IA "diagnostico completo de red" @'
agente plan "Diagnostico de red" como plan
red dns "8.8.8.8" como dns_r
red ping "8.8.8.8" como ping_r
red puerto "google.com" 443 como https_r
red puerto "google.com" 80 como http_r
ia decir "=== DIAGNOSTICO DE RED ==="
ia decir "DNS 8.8.8.8:"
decir $dns_r
ia decir "Ping 8.8.8.8:"
decir $ping_r
ia decir "HTTPS google.com:"
decir $https_r
ia decir "HTTP google.com:"
decir $http_r
agente json plan como resumen
'@

Add-IA "calcular liquidacion de sueldo" @'
agente plan "Liquidacion de sueldo" como plan
agente paso plan hacer "Calcular haberes y deducciones"
guardar sueldo_bruto = 500000
guardar tasa_jub = 0.11
guardar tasa_obra = 0.03
guardar tasa_sind = 0.02
multiplicar $sueldo_bruto $tasa_jub como desc_jub
multiplicar $sueldo_bruto $tasa_obra como desc_obra
multiplicar $sueldo_bruto $tasa_sind como desc_sind
sumar $desc_jub $desc_obra como parcial
sumar $parcial $desc_sind como total_desc
restar $sueldo_bruto $total_desc como sueldo_neto
ia decir "=== LIQUIDACION DE SUELDO ==="
ia decir "Sueldo bruto:"
decir $sueldo_bruto
ia decir "Jubilacion (11%):"
decir $desc_jub
ia decir "Obra social (3%):"
decir $desc_obra
ia decir "Aporte sindical (2%):"
decir $desc_sind
ia decir "Total deducciones:"
decir $total_desc
ia decir "SUELDO NETO:"
decir $sueldo_neto
agente json plan como resumen
'@

Add-IA "crear pagina web para una empresa" @'
agente plan "Crear web corporativa" como plan
agente paso plan hacer "Generar pagina HTML"
web iniciar "Mi Empresa" como pagina
web titulo pagina "Bienvenidos a Nuestra Empresa"
web texto pagina "Somos una empresa dedicada a brindar soluciones de calidad."
web texto pagina "Contactenos para mas informacion."
web boton pagina "Contacto" mensaje "Gracias por contactarnos. Te responderemos pronto."
web guardar pagina en "salida/empresa.html"
ia decir "Pagina web creada en salida/empresa.html"
agente json plan como resumen
'@

Add-IA "generar reporte html" @'
agente plan "Generar reporte HTML" como plan
web iniciar "Reporte de Sistema" como reporte
web titulo reporte "Reporte de Estado del Sistema"
web texto reporte "Generado automaticamente por i@N"
web texto reporte "Estado de la red: verificado"
web boton reporte "Imprimir" mensaje "Use Ctrl+P para imprimir este reporte"
web guardar reporte en "salida/reporte.html"
ia decir "Reporte generado en salida/reporte.html"
agente json plan como resumen
'@

Add-IA "crear lista de tareas pendientes" @'
agente plan "Lista de tareas" como plan
lista crear como tareas
lista agregar tareas "Revisar conectividad de red"
lista agregar tareas "Actualizar firmware Mikrotik"
lista agregar tareas "Hacer backup de configuracion"
lista agregar tareas "Verificar logs del sistema"
lista agregar tareas "Documentar cambios realizados"
lista tamanno tareas como cantidad
ia decir "=== TAREAS PENDIENTES ==="
ia decir "Cantidad de tareas:"
decir $cantidad
lista json tareas como json_tareas
decir $json_tareas
agente json plan como resumen
'@

Add-IA "procesar y analizar texto" @'
agente plan "Analisis de texto" como plan
guardar mi_texto = "Automatizacion con i@N en espanol"
texto longitud $mi_texto como largo
texto mayusculas $mi_texto como upper
texto minusculas $mi_texto como lower
ia decir "=== ANALISIS DE TEXTO ==="
ia decir "Texto original:"
decir $mi_texto
ia decir "Caracteres:"
decir $largo
ia decir "En mayusculas:"
decir $upper
agente json plan como resumen
'@

Add-IA "hacer backup de configuracion mikrotik" @'
agente plan "Backup Mikrotik" como plan
mikrotik script "/system backup save name=backup" como backup_cmd
mikrotik script "/export file=config_export" como export_cmd
ia decir "=== COMANDOS PARA BACKUP MIKROTIK ==="
ia decir "Backup binario (restauracion completa):"
decir $backup_cmd
ia decir "Export de configuracion (texto legible):"
decir $export_cmd
ia decir "Ejecutar estos comandos en la terminal RouterOS."
agente json plan como resumen
'@

Add-IA "configurar firewall mikrotik" @'
ia decir "Reglas basicas de Firewall MikroTik:"
ia decir "PROTEGER EL ROUTER (input chain):"
ia decir "  /ip firewall filter add chain=input connection-state=established,related action=accept"
ia decir "  /ip firewall filter add chain=input connection-state=invalid action=drop"
ia decir "  /ip firewall filter add chain=input in-interface=ether1 action=drop"
ia decir "NAT PARA SALIDA A INTERNET:"
ia decir "  /ip firewall nat add chain=srcnat out-interface=ether1 action=masquerade"
mikrotik script "/ip firewall filter print" como reglas
ia decir "Reglas actuales generadas:"
decir $reglas
'@

Add-IA "contar del 1 al 10" @'
agente plan "Contador" como plan
guardar i = 1
ia decir "Contando del 1 al 10:"
mientras $i menor o igual que 10
  decir $i
  incrementar i
fin
ia decir "Listo!"
agente json plan como resumen
'@

Add-IA "verificar valor con condicion" @'
agente plan "Verificacion condicional" como plan
guardar temperatura = 38
ia decir "=== VERIFICADOR DE TEMPERATURA ==="
ia decir "Temperatura actual:"
decir $temperatura
si $temperatura mayor que 40 entonces
  ia decir "ALERTA: Temperatura critica. Riesgo de dano al equipo."
sino
  si $temperatura mayor que 35 entonces
    ia decir "ADVERTENCIA: Temperatura elevada. Revisar ventilacion."
  sino
    ia decir "OK: Temperatura normal."
  fin
fin
agente json plan como resumen
'@

Add-IA "contame un chiste" @'
ia decir "Por que los programadores confunden Halloween con Navidad?"
ia decir "Porque Oct 31 = Dec 25."
ia decir "(31 en octal = 25 en decimal)"
'@

Add-IA "que es una red informatica" @'
ia decir "Una red informatica conecta dispositivos para compartir recursos e informacion."
ia decir "Tipos de red:"
ia decir "  >> LAN  -> red local (casa, oficina)"
ia decir "  >> WAN  -> red amplia (internet)"
ia decir "  >> WLAN -> red local inalambrica (WiFi)"
ia decir "  >> VPN  -> red privada virtual sobre internet"
ia decir "Componentes clave:"
ia decir "  >> Router      -> conecta redes y dirige trafico"
ia decir "  >> Switch      -> conecta dispositivos en una LAN"
ia decir "  >> Firewall    -> filtra trafico segun reglas"
ia decir "  >> Access Point -> punto de acceso WiFi"
'@

Write-Host ""
$count = ([System.IO.File]::ReadAllLines($memFile, $enc)).Length
Write-Host "=== HECHO ===" -ForegroundColor Green
Write-Host "Memorias totales en brain: $count" -ForegroundColor Green
