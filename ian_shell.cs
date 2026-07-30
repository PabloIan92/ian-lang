using System;
using System.Diagnostics;
using System.IO;

class IanShell
{
    const string Version = "0.1.2";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;

    public static int Run(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding  = System.Text.Encoding.UTF8;
        if (args.Length > 0 && args[0] == "--version") { Console.WriteLine("i@N Shell " + Version); return 0; }
        Greet();
        while (true)
        {
            Console.Write("i@N> ");
            string input = Console.ReadLine();
            if (input == null) return 0;
            input = input.Trim();
            if (input.Length == 0) continue;
            string lower = input.ToLowerInvariant();
            if (lower == "salir" || lower == "exit" || lower == "chau") { Console.WriteLine("Chau. Nos vemos pronto."); return 0; }
            try
            {
                if (lower == "ayuda") { Help(); continue; }
                if (lower.StartsWith("experto ")) { Run("ian.exe", "human perfil " + Q(input.Substring(8))); continue; }
                if (lower == "estado") { Run("ian.exe", "brain estado"); Run("ian.exe", "teacher estado"); continue; }
                if (lower == "vision" || lower == "objetivo") { Run("ian.exe", "brain vision"); continue; }
                if (lower == "laboratorio" || lower == "bien y mal") { Run("ian.exe", "supervisor laboratorio"); continue; }
                if (lower == "supervision claude" || lower == "claude supervisa")
                {
                    AskTeacher("claude", "Supervisa i@n. Dame reglas concretas para mejorar clasificacion, memoria, criterio propio, verificacion, logs y seguridad sin ceder control total.");
                    continue;
                }
                if (lower == "autohospedaje" || lower == "self") { Run("ian.exe", Q(Path.Combine(Home, "self", "bootstrap.ian"))); continue; }
                if (lower == "arquitectura") { Run("ian.exe", "architect mapa"); continue; }
                if (lower == "capacidades" || lower == "pro max") { Run("ian.exe", "architect capacidades"); continue; }
                if (lower == "omnisciencia" || lower == "omnisciente") { Run("ian.exe", "architect omnisciencia"); continue; }
                if (lower == "roadmap") { Run("ian.exe", "architect roadmap"); continue; }
                if (lower == "evolucionar") { Run("ian.exe", "architect sembrar"); Run("ian.exe", "teacher importar"); continue; }
                if (lower == "aprender") { LearnInteractive(); continue; }
                if (lower == "ensenar" || lower == "ense??ar") { Run("ian.exe", "teacher abrir"); continue; }   
                if (lower == "vigilar") { Run("ian.exe", "teacher vigilar"); continue; }
                if (lower == "aprender diario") { Run("ian.exe", "daily"); continue; }
                if (lower.StartsWith("aprender diario ")) { Run("ian.exe", "daily " + Q(input.Substring(16))); continue; }
                if (lower == "api" || lower.StartsWith("api ")) { Run("ian.exe", "api " + (input.Length > 3 ? input.Substring(4) : "--help")); continue; }
                if (MentionsTeacher(lower, "codex")) { AskTeacher("codex", input); continue; }
                if (MentionsTeacher(lower, "claude")) { AskTeacher("claude", input); continue; }
                if (MentionsTeacher(lower, "deepseek")) { AskTeacher("deepseek", input); continue; }
                if (lower.StartsWith("claude ")) { Run("ian.exe", "teacher pedir claude " + Q(input.Substring(7))); continue; }
                if (lower.StartsWith("codex ")) { Run("ian.exe", "teacher pedir codex " + Q(input.Substring(6))); continue; }
                if (lower.StartsWith("deepseek ")) { Run("ian.exe", "teacher pedir deepseek " + Q(input.Substring(9))); continue; }
                if (lower.StartsWith("autonomo ")) { Run("ian.exe", "auto " + Q(input.Substring(9))); continue; }    
                if (lower.StartsWith("evaluar ")) { Run("ian.exe", "supervisor evaluar " + Q(input.Substring(8))); continue; }
                if (lower.StartsWith("mejorar ")) { Run("ian.exe", "supervisor mejorar " + Q(input.Substring(8))); continue; }
                if (lower == "principios") { Run("ian.exe", "supervisor principios"); continue; }
                if (lower.StartsWith("hacer ")) { Run("ian.exe", "agent --run " + Q(input.Substring(6))); continue; }

                // Fallback a Brain para conversacion o ejecucion inteligente
                Run("ian.exe", "brain ejecutar " + Q(input));
            }
            catch (Exception ex) { Console.Error.WriteLine("No pude completar eso: " + ex.Message); }
        }
    }

    static void Greet()
    {
        Console.WriteLine("i@N v" + Version + "  |  ayuda para ver opciones");
    }

    static void Help()
    {
        Console.WriteLine("Podes pedirme cosas asi:");
        Console.WriteLine("  crea una pagina web para mi empresa");
        Console.WriteLine("  prepara un comando Mikrotik para ver interfaces");
        Console.WriteLine("  tengo un problema de DNS, que hago?");
        Console.WriteLine("  experto mikrotik");
        Console.WriteLine("  experto ingenieria");
        Console.WriteLine("  experto matematicas");
        Console.WriteLine("  experto letras");
        Console.WriteLine("  experto fisica");
        Console.WriteLine("  api preguntar wiki MikroTik");
        Console.WriteLine("  arquitectura");
        Console.WriteLine("  capacidades");
        Console.WriteLine("  omnisciencia");
        Console.WriteLine("  vision");
        Console.WriteLine("  laboratorio");
        Console.WriteLine("  supervision claude");
        Console.WriteLine("  deepseek ensenale a i@n sobre matematicas");
        Console.WriteLine();
        Console.WriteLine("Comandos utiles: estado, vision, capacidades, omnisciencia, laboratorio, principios, aprender, ensenar, vigilar, salir.");
    }

    static void LearnInteractive()
    {
        Console.Write("Que queres que aprenda? ");
        string prompt = Console.ReadLine();
        Console.Write("Ruta del archivo .ian con la ense??anza: ");
        string file = Console.ReadLine();
        Run("ian.exe", "brain aprender-archivo " + Q(prompt) + " " + Q(file));
    }

    static bool MentionsTeacher(string lower, string teacher)
    {
        if (!lower.Contains(teacher)) return false;
        return lower.Contains("ense??a") || lower.Contains("ensena") || lower.Contains("aprende") ||
            lower.Contains("mejor") || lower.Contains("ayuda") || lower.Contains("ejecuta") ||
            lower.Contains("usa") || lower.Contains("extrae") || lower.Contains("extraer") ||
            lower.Contains("informacion") || lower.Contains("informaci");
    }

    static void AskTeacher(string teacher, string input)
    {
        Console.WriteLine("Le voy a pedir a " + teacher + " una ensenanza util para i@N.");
        Console.WriteLine("No voy a copiar su modelo interno: voy a extraer reglas, ejemplos y buenas practicas.");
        RunAsync("ian.exe", "teacher pedir " + teacher + " " + Q(input));
        Console.WriteLine("Pedido lanzado en segundo plano. Revisa luego con: estado");
    }

    public static int Run(string exe, string arguments)
    {
        string path = Path.Combine(Home, exe);
        if (!File.Exists(path)) { Console.Error.WriteLine("Error: No se encuentra " + exe); return 1; }
        var psi = new ProcessStartInfo(path, arguments);
        psi.UseShellExecute = false;
        psi.WorkingDirectory = Home;
        using (var p = Process.Start(psi)) { p.WaitForExit(); return p.ExitCode; }
    }

    static void RunAsync(string exe, string arguments)
    {
        string path = Path.Combine(Home, exe);
        if (!File.Exists(path)) { Console.Error.WriteLine("Error: No se encuentra " + exe); return; }
        var psi = new ProcessStartInfo(path, arguments);
        psi.UseShellExecute = true;
        psi.WorkingDirectory = Home;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        Process.Start(psi);
    }

    static string Q(string value) { return "\"" + (value ?? "").Replace("\"", "\\\"") + "\""; }
}

