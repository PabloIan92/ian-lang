using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class IanArchitect
{
    const string Version = "0.1.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string RoadmapDir = Path.Combine(Home, "architecture");
    static readonly string RoadmapFile = Path.Combine(RoadmapDir, "roadmap.md");

    public static int Run(string[] args)
    {
        Directory.CreateDirectory(RoadmapDir);

        if (args.Length == 0 || args[0] == "--help")
        {
            Help();
            return 0;
        }
        if (args[0] == "--version")
        {
            Console.WriteLine("i@N Architect " + Version);
            return 0;
        }

        try
        {
            string cmd = args[0].ToLowerInvariant();
            if (cmd == "mapa") return Map();
            if (cmd == "capacidades") return Capabilities();
            if (cmd == "omnisciencia") return Omniscience();
            if (cmd == "roadmap") return Roadmap();
            if (cmd == "sembrar") return Seed();
            if (cmd == "siguiente") return Next();
            throw new Exception("Comando desconocido: " + args[0]);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N Architect error: " + ex.Message);
            return 1;
        }
    }

    static void Help()
    {
        Console.WriteLine("i@N Architect " + Version);
        Console.WriteLine("Uso:");
        Console.WriteLine("  ian-architect mapa");
        Console.WriteLine("  ian-architect capacidades");
        Console.WriteLine("  ian-architect omnisciencia");
        Console.WriteLine("  ian-architect roadmap");
        Console.WriteLine("  ian-architect sembrar");
        Console.WriteLine("  ian-architect siguiente");
    }

    static int Map()
    {
        Console.WriteLine("Mapa de una IA practica, traducido a i@N:");
        foreach (Part part in Parts())
            Console.WriteLine("- " + part.Name + ": " + part.Goal);
        return 0;
    }

    static int Capabilities()
    {
        Console.WriteLine("Capacidades objetivo i@N Pro Max:");
        Console.WriteLine("1. Conversacion: responder con contexto, memoria y tono natural, no frases fijas.");
        Console.WriteLine("2. Razonamiento: explicar supuestos, comparar opciones, detectar incertidumbre y pedir datos.");
        Console.WriteLine("3. Codigo: crear, leer, modificar, probar y documentar programas.");
        Console.WriteLine("4. Archivos: leer, resumir, transformar y generar documentos locales.");
        Console.WriteLine("5. Herramientas: usar shell, APIs, red, web, Mikrotik, nexos y profesores externos.");
        Console.WriteLine("6. Vision de trabajo: convertir objetivos grandes en planes verificables.");
        Console.WriteLine("7. Memoria: recordar preferencias, reglas, errores, fuentes, fecha y calidad.");
        Console.WriteLine("8. Supervisor: revisar seguridad, calidad, pruebas y riesgo antes de ejecutar.");
        Console.WriteLine("9. Autonomia: ejecutar ciclos objetivo -> plan -> accion -> prueba -> log -> aprendizaje.");
        Console.WriteLine("10. Laboratorio: estudiar riesgo y mal uso por simulacion, sin dano real.");
        Console.WriteLine("11. Multimodal futura: imagen, PDF, voz y datos tabulares cuando haya conectores.");
        Console.WriteLine("12. Producto final: i@N debe demostrar capacidades con pruebas locales, no promesas.");
        return 0;
    }

    static int Omniscience()
    {
        Console.WriteLine("Omnisciencia operativa i@N:");
        Console.WriteLine("Definicion: no saber magicamente todo, sino tener un ciclo para llegar a una respuesta verificable sobre cualquier tema.");
        Console.WriteLine("1. Identificar dominio: ciencia, ingenieria, matematicas, letras, codigo, negocio, salud, ley, red, etc.");
        Console.WriteLine("2. Separar lo sabido, lo incierto y lo que falta.");
        Console.WriteLine("3. Buscar memoria local y conocimiento importado.");
        Console.WriteLine("4. Elegir herramienta: archivos, API, web, shell, profesor externo o laboratorio.");
        Console.WriteLine("5. Responder con supuestos, fuentes o camino de verificacion.");
        Console.WriteLine("6. Guardar aprendizaje con fecha, fuente, calidad y prueba.");
        Console.WriteLine("7. Si no puede verificar, admitir limite y pedir datos.");
        Console.WriteLine("Meta: omnidisciplina verificable, no falsa omnisciencia.");
        return 0;
    }

    static int Roadmap()
    {
        File.WriteAllText(RoadmapFile, BuildRoadmap(), Encoding.UTF8);
        Console.WriteLine("Roadmap creado: " + RoadmapFile);
        return 0;
    }

    static int Seed()
    {
        string manual = Path.Combine(Home, "teach-inbox", "manual");
        Directory.CreateDirectory(manual);
        int count = 0;
        foreach (Part part in Parts())
        {
            string file = Path.Combine(manual, "arquitectura_" + Safe(part.Name) + ".teach");
            File.WriteAllText(file, BuildTeaching(part), Encoding.UTF8);
            count++;
        }
        Console.WriteLine("Ense??anzas arquitectonicas creadas: " + count);
        Console.WriteLine("Importalas con: ian-teacher importar");
        return 0;
    }

    static int Next()
    {
        Console.WriteLine("Siguiente mejora recomendada:");
        Console.WriteLine("Crear un ciclo interno obligatorio antes de ejecutar:");
        Console.WriteLine("1. entender pedido");
        Console.WriteLine("2. clasificar: conversacion, accion, aprendizaje o consulta externa");
        Console.WriteLine("3. planificar pasos");
        Console.WriteLine("4. revisar con supervisor");
        Console.WriteLine("5. ejecutar");
        Console.WriteLine("6. guardar memoria y log");
        return 0;
    }

    static string BuildRoadmap()
    {
        var b = new StringBuilder();
        b.AppendLine("# Arquitectura Evolutiva i@N");
        b.AppendLine();
        b.AppendLine("Este mapa no copia una IA externa. Define piezas propias para que i@N crezca con control local.");
        b.AppendLine();
        foreach (Part p in Parts())
        {
            b.AppendLine("## " + p.Name);
            b.AppendLine();
            b.AppendLine("Objetivo: " + p.Goal);
            b.AppendLine();
            b.AppendLine("Version superior: " + p.Superior);
            b.AppendLine();
            b.AppendLine("Estado actual: " + p.Current);
            b.AppendLine();
        }
        return b.ToString();
    }

    static string BuildTeaching(Part part)
    {
        return
            "# pregunta: arquitectura " + part.Name + "\n" +
            "agente plan \"Construir modulo " + part.Name + "\" como plan\n" +
            "agente paso plan hacer \"Objetivo: " + Escape(part.Goal) + "\"\n" +
            "agente paso plan hacer \"Version superior: " + Escape(part.Superior) + "\"\n" +
            "agente paso plan hacer \"Integrar con memoria, supervisor y logs\"\n" +
            "agente paso plan hacer \"Medir si mejora la respuesta antes de ejecutarla\"\n" +
            "agente json plan como resumen\n" +
            "decir $resumen\n";
    }

    static List<Part> Parts()
    {
        return new List<Part> {
            new Part("entrada", "recibir texto humano, comandos y archivos", "entender intencion sin obligar al usuario a usar comandos rigidos", "i@n shell"),
            new Part("clasificador", "separar conversacion, accion, aprendizaje, API y codigo", "evitar mandar charla al generador de programas", "reglas simples en ian-shell"),
            new Part("memoria", "recordar ense??anzas locales", "usar memoria por relevancia, fecha, fuente y calidad", "ian-brain"),
            new Part("planificador", "convertir objetivos en pasos", "crear planes verificables antes de actuar", "agente plan"),
            new Part("herramientas", "ejecutar motor, API, red, web, mikrotik y nexos", "elegir herramienta por necesidad y explicar resultado", "ian.exe y conectores"),
            new Part("supervisor", "criticar calidad y seguridad", "bloquear o mejorar acciones flojas antes de ejecutarlas", "ian-supervisor"),
            new Part("aprendizaje", "aprender de Codex, Claude, APIs y archivos", "extraer solo conocimiento util, no ruido", "ian-teacher y ian-daily"),
            new Part("autonomia", "actuar por objetivos y registrar logs", "hacer ciclos repetibles: pensar, ejecutar, revisar, aprender", "ian-auto"),
            new Part("personalidad", "hablar de forma natural", "ser claro, breve, no robotico y admitir limites", "ian-human"),
            new Part("seguridad", "evitar da??o y acciones confusas", "pedir aclaracion si hay riesgo o ambiguedad", "reglas de supervisor")
        };
    }

    static string Safe(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "_");
    }

    static string Escape(string text)
    {
        return (text ?? "").Replace("\"", "'");
    }
}

class Part
{
    public string Name;
    public string Goal;
    public string Superior;
    public string Current;

    public Part(string name, string goal, string superior, string current)
    {
        Name = name;
        Goal = goal;
        Superior = superior;
        Current = current;
    }
}

