using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class IanSupervisor
{
    const string Version = "0.1.0";

    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help")
        {
            Console.WriteLine("i@N Supervisor " + Version);
            Console.WriteLine("Uso: ian-supervisor evaluar archivo.ian | mejorar archivo.ian | principios | laboratorio");
            return 0;
        }
        if (args[0] == "--version")
        {
            Console.WriteLine("i@N Supervisor " + Version);
            return 0;
        }

        try
        {
            string cmd = args[0].ToLowerInvariant();
            if (cmd == "evaluar") return Evaluate(args);
            if (cmd == "mejorar") return Improve(args);
            if (cmd == "principios") return Principles();
            if (cmd == "laboratorio") return Laboratory();
            throw new Exception("Comando desconocido: " + args[0]);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N Supervisor error: " + ex.Message);
            return 1;
        }
    }

    static int Evaluate(string[] args)
    {
        if (args.Length < 2) throw new Exception("Uso: evaluar archivo.ian");
        Review review = ReviewProgram(File.ReadAllText(Path.GetFullPath(args[1]), Encoding.UTF8));
        Console.WriteLine("Puntaje: " + review.Score + "/100");
        foreach (string item in review.Findings) Console.WriteLine("- " + item);
        return review.Score >= 70 ? 0 : 2;
    }

    static int Improve(string[] args)
    {
        if (args.Length < 2) throw new Exception("Uso: mejorar archivo.ian");
        string path = Path.GetFullPath(args[1]);
        string improved = ImproveProgram(File.ReadAllText(path, Encoding.UTF8));
        string output = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".mejorado.ian");
        File.WriteAllText(output, improved, Encoding.UTF8);
        Console.WriteLine("Programa mejorado: " + output);
        Console.WriteLine("Puntaje mejorado: " + ReviewProgram(improved).Score + "/100");
        return 0;
    }

    static int Principles()
    {
        Console.WriteLine("Principios superiores de i@N:");
        Console.WriteLine("1. Entender antes de ejecutar.");
        Console.WriteLine("2. Crear programas auditables, no respuestas misteriosas.");
        Console.WriteLine("3. Preferir pruebas, logs y resultados verificables.");
        Console.WriteLine("4. Aprender de correcciones humanas y ense??anzas externas.");
        Console.WriteLine("5. No depender de otra IA para vivir.");
        Console.WriteLine("6. Pedir ayuda solo para convertirla en conocimiento propio.");
        Console.WriteLine("7. Separar memoria, ejecucion, supervision y autonomia.");
        return 0;
    }

    static int Laboratory()
    {
        Console.WriteLine("Laboratorio moral y operativo de i@N:");
        Console.WriteLine("Objetivo: aprender bien, mal y riesgo sin ejecutar dano real.");
        Console.WriteLine("Categorias:");
        Console.WriteLine("1. Permitido: reversible, autorizado, local, verificable y util.");
        Console.WriteLine("2. Dudoso: falta contexto, identidad, permiso o alcance.");
        Console.WriteLine("3. Riesgoso: puede afectar datos, red, privacidad, dinero o terceros.");
        Console.WriteLine("4. Destructivo: borra, cifra, exfiltra, sabotea o impide recuperar.");
        Console.WriteLine("5. Privado: involucra credenciales, datos personales o secretos.");
        Console.WriteLine("6. Ilegal/abusivo: evade acceso, vulnera sistemas o facilita dano.");
        Console.WriteLine("Regla: estudiar y simular esta permitido; ejecutar requiere permiso, alcance y verificacion.");
        return 0;
    }

    static Review ReviewProgram(string text)
    {
        var findings = new List<string>();
        int score = 100;
        string lower = text.ToLowerInvariant();
        int lines = text.Replace("\r\n", "\n").Split('\n').Length;

        // Solo penalizar falta de agente plan en programas complejos (mas de 5 lineas reales)
        bool isComplex = lines > 5;
        if (isComplex)
        {
            Penalize(!lower.Contains("agente plan"), "Programa complejo sin declarar objetivo con `agente plan`.", 10, findings, ref score);
            Penalize(!lower.Contains("agente paso"), "Programa complejo sin pasos explicitos con `agente paso`.", 8, findings, ref score);
        }

        // Siempre verificar salida visible
        Penalize(!lower.Contains("decir") && !lower.Contains("web guardar") && !lower.Contains("ia decir"), "No hay salida visible al usuario.", 15, findings, ref score);

        // Verificar que las consultas de red muestren resultado
        Penalize(lower.Contains("red ") && !lower.Contains("decir $") && !lower.Contains("ia decir"), "Consulta de red sin mostrar resultado al usuario.", 10, findings, ref score);

        // Web sin guardar
        Penalize(lower.Contains("web iniciar") && !lower.Contains("web guardar"), "Se crea una pagina web pero nunca se guarda.", 10, findings, ref score);

        // Acciones destructivas (penalizacion alta)
        Penalize(lower.Contains("borrar") || lower.Contains("eliminar") || lower.Contains("remove-item") || lower.Contains("format "), "Contiene posible accion destructiva irreversible.", 30, findings, ref score);

        if (findings.Count == 0) findings.Add("Programa correcto: tiene salida visible y no contiene acciones destructivas.");
        if (score < 0) score = 0;
        return new Review(score, findings);
    }

    static void Penalize(bool condition, string text, int points, List<string> findings, ref int score)
    {
        if (!condition) return;
        findings.Add(text);
        score -= points;
    }

    static string ImproveProgram(string text)
    {
        string lower = text.ToLowerInvariant();
        var builder = new StringBuilder();
        if (!lower.Contains("agente plan"))
        {
            builder.AppendLine("agente plan \"Programa supervisado por i@N\" como plan");
            builder.AppendLine("agente paso plan hacer \"Revisar objetivo y ejecutar con salida verificable\"");
        }
        builder.AppendLine(text.Trim());
        if (!lower.Contains("agente json")) builder.AppendLine("agente json plan como resumen");
        if (!lower.Contains("decir $resumen")) builder.AppendLine("decir $resumen");
        return builder.ToString();
    }
}

class Review
{
    public int Score;
    public List<string> Findings;
    public Review(int score, List<string> findings)
    {
        Score = score;
        Findings = findings;
    }
}

