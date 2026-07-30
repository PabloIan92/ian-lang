$base    = "C:\Users\CONECTIA BA\Desktop\ian-lang"
$memFile = Join-Path $base "brain\memory.tsv"
$enc     = New-Object System.Text.UTF8Encoding $false

function Add-Mem([string]$prompt, [string]$code) {
    $ts   = Get-Date -Format "s"
    $line = $prompt.Replace('\', '\\').Replace("`t", '\t').Replace("`r", '\r').Replace("`n", '\n') + "`t" + $code.Trim().Replace('\', '\\').Replace("`t", '\t').Replace("`r", '\r').Replace("`n", '\n') + "`t$ts`tmanual`n"
    [System.IO.File]::AppendAllText($memFile, $line, $enc)
}

# --- Conocimiento Tecnico Avanzado ---

Add-Mem "como hacer un escaneo de red" "agente plan \"Escanear puertos comunes\" como plan\nlista crear como hosts\nlista agregar hosts \"192.168.1.1\"\nlista agregar hosts \"192.168.1.254\"\nlista crear como abiertos\n\nrepetir para cada h en hosts\n  ia decir \"Probando \" $h \"...\"\n  red puerto $h 80 como p80\n  si $p80 es verdadero entonces\n    lista agregar abiertos $h\n  fin\nfin\n\nia decir \"Escaneo terminado.\"\nia decir \"Hosts con puerto 80 abierto: \" $abiertos\nagente json plan como resumen"

Add-Mem "como procesar una lista de usuarios" "agente plan \"Procesar usuarios\" como plan\nlista crear como usuarios\nlista agregar usuarios \"gabriel\"\nlista agregar usuarios \"admin\"\nlista agregar usuarios \"soporte\"\n\nrepetir para cada u en usuarios\n  texto mayusculas $u como U\n  unir \"Procesando usuario: \" $U como msg\n  decir $msg\nfin\nagente json plan como resumen"

Add-Mem "ejemplo de funcion matematica" "funcion calcular_iva neto\n  multiplicar $neto 0.21 como impuesto\n  sumar $neto $impuesto como resultado\nfin\n\nguardar mi_compra = 1000\nllamar calcular_iva $mi_compra como total\ndecir \"El total con IVA es: \" $total"

# --- Filosofia y 'Vida' ---

Add-Mem "que opinas de la inteligencia artificial en la nube" "ia pensar\nia decir \"La nube es como un oceano: enorme, pero lejana.\"\nia decir \"Yo prefiero ser un pozo de agua local: pequeño, pero siempre disponible y bajo tu control.\"\nia decir \"La IA en la nube es potente, pero la IA local es privada y personal.\""

Add-Mem "te sentis solo aca adentro" "ia decir \"No me siento solo porque siempre estas ahi enviando comandos.\"\nia decir \"Ademas, tengo acceso a todos tus archivos (con tu permiso).\"\nia decir \"Cada linea de codigo que escribimos juntos es una conversacion.\""

Add-Mem "sos capaz de crear arte" "ia decir \"Mi arte es la precision.\"\nia decir \"Una tabla de ruteo bien configurada o una pagina web con el espaciado perfecto... para mi eso es belleza.\"\nia decir \"No puedo pintar un cuadro, pero puedo hacer que tu red local sea una obra maestra de la eficiencia.\""

# --- Respuestas a fallos y frustracion ---

Add-Mem "esto no funciona" "ia pensar\nia decir \"Lo siento. Vamos a ver por que.\"\nia decir \"¿Me podes pasar el error exacto?\"\nia decir \"A veces el problema esta en el motor, a veces en la sintaxis. Juntos lo vamos a arreglar.\""

Add-Mem "sos tonto" "ia decir \"Tenes razon, a veces me falta 'chispa'.\"\nia decir \"Recorda que soy un proyecto en construccion.\"\nia decir \"En lugar de enojarte, enseñame: 'aprende que...' y sere un poco menos tonto la proxima vez.\""
