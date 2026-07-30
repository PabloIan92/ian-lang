using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class IanHuman
{
    const string Version = "0.1.1";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;

    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help")
        {
            Help();
            return 0;
        }
        if (args[0] == "--version")
        {
            Console.WriteLine("i@N Human " + Version);
            return 0;
        }

        string cmd = args[0].ToLowerInvariant();
        if (cmd == "perfil") return Profile(args);
        if (cmd == "responder") return Answer(args);
        if (cmd == "areas") return Areas();

        Console.Error.WriteLine("No entendi ese comando.");
        return 1;
    }

    static void Help()
    {
        Console.WriteLine("i@N Human " + Version);
        Console.WriteLine("Uso: ian-human responder \"pregunta\"");
    }

    static int Areas()
    {
        Console.WriteLine("Areas iniciales: redes, mikrotik, web, programacion, ia, negocio, ingenieria, matematicas, letras, fisica, general.");
        return 0;
    }

    static int Profile(string[] args)
    {
        string area = args.Length >= 2 ? Normalize(args[1]) : "general";
        Console.WriteLine(GetProfile(area));
        return 0;
    }

    static int Answer(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Decime la pregunta y te respondo.");
            return 0;
        }

        string question = string.Join(" ", Slice(args, 1));
        Console.WriteLine(BuildAnswer(question, DetectArea(question)));
        return 0;
    }

    static string BuildAnswer(string question, string area)
    {
        string q = Normalize(question);

        // --- Nuevos patrones: identidad y meta ---
        if (Has(q, "quien te creo", "quien te hizo", "quien eres", "qui??n te cre??", "qui??n te hizo", "qui??n eres"))
            return "Soy un proyecto de IA local construido con Claude y Codex. Corro directamente en tu PC.";

        if (Has(q, "sos una ia", "eres una ia", "que tipo de ia", "inteligencia artificial"))
            return "Si, soy una IA local. No tengo un modelo neuronal todavia, pero aprendo de ejemplos y guardo lo que se en archivos propios.";

        if (Has(q, "que sabes de mi", "me conoces", "sabes quien soy"))
            return "Solo se lo que me contas en esta conversacion. No guardo datos personales entre sesiones. Decime tu nombre si queres que lo use.";

        if (Has(q, "hablas como robot", "pareces robot", "sos un robot", "habla normal", "habla como persona"))
            return "Tenes razon. Voy a intentar ser mas directo y menos formal. Decime que necesitas.";

        if (Has(q, "gracias", "muchas gracias"))
            return "De nada.";

        // --- Patrones existentes (reescritos) ---
        if (Has(q, "modelo de programacion", "como te crearon", "como fuiste creado", "como estas hecho", "con que estas hecho", "que modelo se uso"))
            return "Me hicieron en C#/.NET, con partes separadas: motor, agente, cerebro, profesor y consola. Cada parte se puede revisar y mejorar.";

        if (Has(q, "vida propia", "no queres", "no quieres", "por que no", "ser ia"))
            return "Si, soy una IA local. No tengo un modelo neuronal todavia, pero aprendo de ejemplos y guardo lo que se en archivos propios.";

        if (Has(q, "hablar normal", "habla normal", "conversa normal", "como persona", "sos robot", "eres robot", "robotico"))
            return "Tenes razon. Voy a intentar ser mas directo y menos formal. Decime que necesitas.";

        if (Has(q, "experto", "materia", "todo"))
            return "Puedo trabajar por areas y aprender, pero no voy a inventar que se todo. Si falta conocimiento, te lo digo y busco la forma de conseguirlo.";

        if (Has(q, "omnisciente", "saber todo", "sabes todo"))
            return "No soy omnisciente. Puedo aspirar a ser multi-experto verificable: responder por areas, declarar limites, usar fuentes y probar lo que afirmo.";

        if (area == "redes")
            return "Para redes miraria primero DNS, puerto, conectividad o firewall. Decime el sintoma y te genero una comprobacion.";

        if (area == "mikrotik")
            return "En Mikrotik puedo ayudarte con interfaces, IP, rutas, firewall, NAT y scripts. Decime que necesitas.";

        if (area == "web")
            return "Para web puedo generarte una pagina HTML ahora mismo. Decime el nombre o tema.";

        if (area == "programacion")
            return "Me hicieron en C#/.NET, con partes separadas: motor, agente, cerebro, profesor y consola. Cada parte se puede revisar y mejorar.";

        if (area == "ingenieria")
            return "Modo ingenieria: primero defino requisitos, restricciones, seguridad, calculos, pruebas y mantenimiento. Si falta una norma o dato, lo pido.";

        if (area == "matematicas")
            return "Modo matematicas: planteo definiciones, hipotesis, demostracion o calculo paso a paso, y verifico el resultado.";

        if (area == "letras")
            return "Modo letras: reviso estructura, tono, estilo, argumento, retorica, gramatica y contexto cultural sin inventar citas.";

        if (area == "fisica")
            return "Modo fisica: identifico sistema, variables, leyes aplicables, unidades, aproximaciones y validacion dimensional.";

        if (area == "ia")
            return "Soy una IA local. Aprendo de ejemplos, no entreno pesos. Lo que se esta en archivos que podes ver y editar.";

        if (area == "negocio")
            return "Para un negocio miraria objetivo, cliente, oferta y siguiente accion. Puedo convertirlo en un plan.";

        // --- Fallback: preguntar a Claude ---
        try
        {
            string claude = AskClaude(question);
            if (!string.IsNullOrEmpty(claude)) return claude;
        }
        catch {}
        return "No entiendo bien ese pedido. Reformulalo o escribi ayuda.";
    }

    static string AskClaude(string question)
    {
        try
        {
            string prompt = "Sos i@N, un asistente tecnico local que corre en esta PC. Respond??s en espa??ol rioplatense (vos, che, etc.), en 1-2 oraciones maximas, natural y sin formalismos. No menciones que sos Claude. La pregunta del usuario es: " + question;

            var psi = new ProcessStartInfo();
            psi.FileName = "claude";
            psi.Arguments = "-p \"" + prompt.Replace("\"", "\\\"") + "\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = Home;

            string stdout = "";
            string stderr = "";

            using (var p = Process.Start(psi))
            {
                var outThread = new Thread(() => { stdout = p.StandardOutput.ReadToEnd(); });
                var errThread = new Thread(() => { stderr = p.StandardError.ReadToEnd(); });
                outThread.Start();
                errThread.Start();

                bool exited = p.WaitForExit(20000);
                outThread.Join(5000);
                errThread.Join(5000);

                if (!exited) return null;
            }

            string result = (stdout ?? "").Trim();
            if (string.IsNullOrEmpty(result)) return null;

            // Strip markdown code fences if present
            if (result.StartsWith("```"))
            {
                int firstNewline = result.IndexOf('\n');
                if (firstNewline >= 0) result = result.Substring(firstNewline + 1);
                if (result.EndsWith("```")) result = result.Substring(0, result.Length - 3).TrimEnd();
            }

            return result.Trim();
        }
        catch
        {
            return null;
        }
    }

    static string GetProfile(string area)
    {
        if (area == "redes") return "Modo redes: DNS, puertos, rutas, firewall, latencia y disponibilidad.";
        if (area == "mikrotik") return "Modo Mikrotik: RouterOS, interfaces, bridge, IP, firewall, NAT, DHCP, DNS, queues y scripts.";
        if (area == "web") return "Modo web: HTML, UX, responsive, contenido y publicacion.";
        if (area == "programacion") return "Modo programacion: claridad, pruebas, errores utiles, mantenimiento y automatizacion.";
        if (area == "ingenieria") return "Modo ingenieria: requisitos, restricciones, calculos, seguridad, pruebas, normas y mantenimiento.";
        if (area == "matematicas") return "Modo matematicas: definiciones, calculo, demostracion, verificacion y contraejemplos.";
        if (area == "letras") return "Modo letras: estilo, gramatica, literatura, retorica, interpretacion y edicion.";
        if (area == "fisica") return "Modo fisica: modelos, leyes, unidades, aproximaciones, experimentos y validacion.";
        if (area == "ia") return "Modo IA: memoria local, aprendizaje, agentes, supervision y autonomia.";
        if (area == "negocio") return "Modo negocio: clientes, propuesta de valor, costos, ventas y decisiones.";
        return "Modo general: conversar claro, pedir contexto si falta y convertir pedidos en acciones verificables.";
    }

    static string DetectArea(string text)
    {
        string q = Normalize(text);
        if (Has(q, "mikrotik", "routeros", "router", "winbox")) return "mikrotik";
        if (Has(q, "red", "dns", "puerto", "ip", "firewall", "servidor", "ping")) return "redes";
        if (Has(q, "web", "pagina", "html", "css", "sitio", "landing")) return "web";
        if (Has(q, "programa", "programacion", "codigo", "funcion", "bug", "software", "app", "modelo")) return "programacion";
        if (Has(q, "ingenieria", "ingeniero", "estructuras", "mecanica", "electrica", "civil", "industrial")) return "ingenieria";
        if (Has(q, "matematica", "matematicas", "calculo", "algebra", "geometria", "teorema", "demostracion")) return "matematicas";
        if (Has(q, "letras", "literatura", "gramatica", "poesia", "novela", "ensayo", "retorica")) return "letras";
        if (Has(q, "fisica", "energia", "fuerza", "masa", "velocidad", "cuantica", "termodinamica")) return "fisica";
        if (Has(q, "ia", "inteligencia", "agente", "autonomo", "aprende")) return "ia";
        if (Has(q, "negocio", "empresa", "cliente", "venta", "precio", "marketing")) return "negocio";
        return "general";
    }

    static bool Has(string text, params string[] words)
    {
        foreach (string word in words)
            if (text.Contains(Normalize(word))) return true;
        return false;
    }

    static string Normalize(string text)
    {
        return (text ?? "").ToLowerInvariant()
            .Replace("_", " ")
            .Replace("??", "a").Replace("??", "e").Replace("??", "i")
            .Replace("??", "o").Replace("??", "u").Replace("??", "n");
    }

    static string[] Slice(string[] input, int start)
    {
        if (start >= input.Length) return new string[0];
        string[] output = new string[input.Length - start];
        for (int i = start; i < input.Length; i++) output[i - start] = input[i];
        return output;
    }
}

