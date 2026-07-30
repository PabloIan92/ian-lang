using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

class IanAgent
{
    const string Version = "0.2.0";

    public static int Run(string[] args)
    {
        bool run = false;
        bool repl = false;
        var promptParts = new List<string>();

        foreach (string arg in args)
        {
            if (arg == "--run") run = true;
            else if (arg == "--chat") repl = true;
            else if (arg == "--version")
            {
                Console.WriteLine("i@N Agent " + Version);
                return 0;
            }
            else promptParts.Add(arg);
        }

        if (repl || promptParts.Count == 0)
        {
            return Chat(run);
        }

        return BuildAndMaybeRun(string.Join(" ", promptParts.ToArray()), run);
    }

    static int Chat(bool run)
    {
        Console.WriteLine("i@N Agent " + Version);
        Console.WriteLine("Escribi un pedido. Para salir: salir");
        while (true)
        {
            Console.Write("i@N> ");
            string line = Console.ReadLine();
            if (line == null) return 0;
            line = line.Trim();
            if (line.Length == 0) continue;
            if (line.Equals("salir", StringComparison.OrdinalIgnoreCase)) return 0;
            BuildAndMaybeRun(line, run);
        }
    }

    static int BuildAndMaybeRun(string prompt, bool run)
    {
        try
        {
            string home = AppDomain.CurrentDomain.BaseDirectory;
            string generatedDir = Path.Combine(home, "generated");
            Directory.CreateDirectory(generatedDir);

            string program = CompilePrompt(prompt);
            string file = Path.Combine(generatedDir, "agent_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".ian");
            File.WriteAllText(file, program, Encoding.UTF8);

            Console.WriteLine("Programa i@N generado:");
            Console.WriteLine(file);
            Console.WriteLine();
            Console.WriteLine(program);

            if (run)
            {
                Console.WriteLine();
                Console.WriteLine("Ejecucion:");
                return RunIan(home, file);
            }

            Console.WriteLine("Para ejecutarlo:");
            Console.WriteLine(".\\ian.exe .\\generated\\agent_program.ian");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N Agent error: " + ex.Message);
            return 1;
        }
    }

    static string CompilePrompt(string prompt)
    {
        string p = Normalize(prompt);
        var code = new StringBuilder();

        code.AppendLine("# Generado por i@N Agent");
        code.Append("agente plan ").Append(Q("Resolver pedido: " + prompt)).AppendLine(" como plan");

        bool didSomething = false;

        if (HasAny(p, "web", "pagina", "p??gina", "html", "sitio"))
        {
            didSomething = true;
            string title = ExtractAfter(prompt, "para", "Pagina creada con i@N");
            code.AppendLine("agente paso plan hacer \"Crear estructura web\"");
            code.Append("web iniciar ").Append(Q(title)).AppendLine(" como pagina");
            code.Append("web titulo pagina ").Append(Q(title)).AppendLine();
            code.Append("web texto pagina ").Append(Q("Esta pagina fue generada por i@N Agent desde un pedido en lenguaje natural.")).AppendLine();
            code.AppendLine("web boton pagina \"Probar\" mensaje \"i@N Agent genero esta pagina\"");
            code.AppendLine("web guardar pagina en \"salida/agent-web.html\"");
        }

        if (HasAny(p, "dns", "dominio"))
        {
            didSomething = true;
            string domain = ExtractDomain(prompt);
            code.AppendLine("agente paso plan hacer \"Consultar DNS\"");
            code.Append("red dns ").Append(Q(domain)).AppendLine(" como dns");
            code.AppendLine("decir $dns");
        }

        if (HasAny(p, "puerto", "port", "servidor", "http", "https"))
        {
            didSomething = true;
            string host = ExtractDomain(prompt);
            int port = HasAny(p, "https", "443") ? 443 : 80;
            code.AppendLine("agente paso plan hacer \"Comprobar puerto de red\"");
            code.Append("red puerto ").Append(Q(host)).Append(" ").Append(port).AppendLine(" como puerto_abierto");
            code.AppendLine("decir $puerto_abierto");
        }

        if (HasAny(p, "mikrotik", "routeros", "router", "interface", "interfaz", "ip address"))
        {
            didSomething = true;
            code.AppendLine("agente paso plan hacer \"Preparar comando Mikrotik RouterOS\"");
            if (HasAny(p, "interface", "interfaz"))
                code.AppendLine("mikrotik script \"/interface print\" como consulta_mikrotik");
            else
                code.AppendLine("mikrotik script \"/ip address print\" como consulta_mikrotik");
            code.AppendLine("decir $consulta_mikrotik");
        }

        if (HasAny(p, "python", "nexo", "otro lenguaje", "comunicar"))
        {
            didSomething = true;
            code.AppendLine("agente paso plan hacer \"Usar nexo con otro lenguaje\"");
            code.AppendLine("nexo mayusculas = proceso \"python ../tools/upper_bridge.py\"");
            code.Append("llamar mayusculas con ").Append(Q(prompt)).AppendLine(" como respuesta_nexo");
            code.AppendLine("decir $respuesta_nexo");
        }

        if (HasAny(p, "calcular", "calculo", "suma", "resta", "multiplica", "divide", "liquidacion", "sueldo", "jornal", "salario"))
        {
            didSomething = true;
            code.AppendLine("agente paso plan hacer \"Realizar c??lculos\"");
            code.AppendLine("guardar valor_a = 100");
            code.AppendLine("guardar valor_b = 25");
            code.AppendLine("sumar $valor_a $valor_b como suma");
            code.AppendLine("restar $valor_a $valor_b como resta");
            code.AppendLine("multiplicar $valor_a $valor_b como producto");
            code.AppendLine("decir $suma");
            code.AppendLine("decir $resta");
            code.AppendLine("decir $producto");
        }

        if (HasAny(p, "lista", "listar", "enumerar", "agregar items", "elementos"))
        {
            didSomething = true;
            code.AppendLine("agente paso plan hacer \"Crear y manejar lista\"");
            code.AppendLine("lista crear como elementos");
            code.AppendLine("lista agregar elementos \"Item 1\"");
            code.AppendLine("lista agregar elementos \"Item 2\"");
            code.AppendLine("lista agregar elementos \"Item 3\"");
            code.AppendLine("lista tamanno elementos como cantidad");
            code.AppendLine("decir $cantidad");
            code.AppendLine("lista json elementos como resultado");
            code.AppendLine("decir $resultado");
        }

        if (HasAny(p, "si", "condicion", "verificar", "comprobar", "chequear", "validar"))
        {
            didSomething = true;
            code.AppendLine("agente paso plan hacer \"Verificar condici??n\"");
            code.AppendLine("guardar valor = 42");
            code.AppendLine("si $valor mayor que 0 entonces");
            code.AppendLine("  decir \"El valor es positivo\"");
            code.AppendLine("sino");
            code.AppendLine("  decir \"El valor es cero o negativo\"");
            code.AppendLine("fin");
        }

        if (HasAny(p, "texto", "string", "cadena", "mayuscula", "minuscula", "concatenar", "unir texto"))
        {
            didSomething = true;
            string textVal = ExtractAfter(prompt, "texto", ExtractAfter(prompt, "string", "hola mundo"));
            code.AppendLine("agente paso plan hacer \"Manipular texto\"");
            code.Append("guardar mi_texto = ").AppendLine(Q(textVal));
            code.AppendLine("texto longitud $mi_texto como largo");
            code.AppendLine("texto mayusculas $mi_texto como upper");
            code.AppendLine("decir $largo");
            code.AppendLine("decir $upper");
        }

        if (!didSomething)
        {
            code.AppendLine("agente paso plan hacer \"Analizar pedido\"");
            code.Append("decir ").AppendLine(Q("i@N Agent v0.2 no tiene una habilidad espec??fica para este pedido."));
            code.Append("decir ").AppendLine(Q("Habilidades disponibles: web, red DNS/puertos, mikrotik, c??lculos, listas, texto, python/nexo."));
        }

        code.AppendLine("agente json plan como resumen");
        code.AppendLine("decir $resumen");
        return code.ToString();
    }

    static int RunIan(string home, string file)
    {
        string engine = Path.Combine(home, "ian.exe");
        var psi = new ProcessStartInfo();
        psi.FileName = engine;
        psi.Arguments = "--allow-run " + QuoteArg(file);
        psi.UseShellExecute = false;
        psi.WorkingDirectory = Path.GetDirectoryName(file);
        using (var process = Process.Start(psi))
        {
            process.WaitForExit();
            return process.ExitCode;
        }
    }

    static bool HasAny(string text, params string[] words)
    {
        foreach (string word in words)
            if (text.Contains(Normalize(word))) return true;
        return false;
    }

    static string Normalize(string text)
    {
        return (text ?? "")
            .ToLowerInvariant()
            .Replace("??", "a").Replace("??", "e").Replace("??", "i")
            .Replace("??", "o").Replace("??", "u").Replace("??", "n");
    }

    static string ExtractDomain(string prompt)
    {
        string[] parts = prompt.Split(new char[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string raw in parts)
        {
            string item = raw.Trim().Trim('"', '\'').Replace("https://", "").Replace("http://", "").Trim('/');
            if (item.Contains(".") && !item.EndsWith(".ian", StringComparison.OrdinalIgnoreCase))
                return item;
        }
        return "example.com";
    }

    static string ExtractAfter(string prompt, string marker, string fallback)
    {
        int index = Normalize(prompt).IndexOf(Normalize(marker));
        if (index < 0) return fallback;
        string value = prompt.Substring(index + marker.Length).Trim();
        if (value.Length == 0) return fallback;
        value = CutBefore(value, " y revisa ");
        value = CutBefore(value, " y verifica ");
        value = CutBefore(value, " y comprueba ");
        value = CutBefore(value, " y chequea ");
        if (value.Length > 70) value = value.Substring(0, 70).Trim();
        return value;
    }

    static string CutBefore(string value, string marker)
    {
        int index = Normalize(value).IndexOf(Normalize(marker));
        if (index < 0) return value;
        return value.Substring(0, index).Trim();
    }

    static string Q(string value)
    {
        return "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    static string QuoteArg(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
    }
}

