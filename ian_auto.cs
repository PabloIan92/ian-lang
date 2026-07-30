using System;
using System.Diagnostics;
using System.IO;
using System.Text;

class IanAuto
{
    const string Version = "0.1.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string Logs = Path.Combine(Home, "autonomy-logs");

    public static int Run(string[] args)
    {
        Directory.CreateDirectory(Logs);

        if (args.Length == 0 || args[0] == "--help")
        {
            Help();
            return 0;
        }

        if (args[0] == "--version")
        {
            Console.WriteLine("i@N Auto " + Version);
            return 0;
        }

        bool askExternal = false;
        int start = 0;
        if (args[0] == "--ask-external")
        {
            askExternal = true;
            start = 1;
        }

        string goal = string.Join(" ", Slice(args, start));
        if (string.IsNullOrWhiteSpace(goal))
        {
            Console.Error.WriteLine("i@N Auto error: falta objetivo");
            return 1;
        }

        return RunGoal(goal, askExternal);
    }

    static void Help()
    {
        Console.WriteLine("i@N Auto " + Version);
        Console.WriteLine("Uso:");
        Console.WriteLine("  ian.exe auto \"objetivo\"");
        Console.WriteLine("  ian.exe auto --ask-external \"objetivo\"");
        Console.WriteLine();
        Console.WriteLine("Hace un ciclo local: pensar -> crear programa i@N -> ejecutar -> registrar.");
    }

    static int RunGoal(string goal, bool askExternal)
    {
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string log = Path.Combine(Logs, "auto_" + stamp + ".log");
        var report = new StringBuilder();
        report.AppendLine("Objetivo: " + goal);
        report.AppendLine("Fecha: " + DateTime.Now.ToString("s"));
        report.AppendLine();

        Console.WriteLine("i@N Auto despierto.");
        Console.WriteLine("Objetivo: " + goal);

        string brainOutput = RunCapture("ian.exe", "brain crear " + Q(goal), report);
        string generated = ExtractGeneratedPath(brainOutput);

        if (string.IsNullOrWhiteSpace(generated) || !File.Exists(generated))
        {
            report.AppendLine("Brain no genero archivo ejecutable. Uso Agent como respaldo.");
            string agentOutput = RunCapture("ian.exe", "agent --run " + Q(goal), report);
            report.AppendLine(agentOutput);
        }
        else
        {
            Console.WriteLine("Programa creado: " + generated);
            report.AppendLine("Programa creado: " + generated);
            string runOutput = RunCapture("ian.exe", Q(generated), report);
            Console.WriteLine(runOutput.Trim());
            report.AppendLine(runOutput);
        }

        if (askExternal)
        {
            report.AppendLine();
            report.AppendLine("Pidiendo mejora externa a Codex...");
            try
            {
                string teach = RunCapture("ian.exe", "teacher pedir codex " + Q("Ensenale a i@N a mejorar este objetivo: " + goal), report);
                report.AppendLine(teach);
            }
            catch (Exception ex)
            {
                report.AppendLine("No se pudo pedir ayuda externa: " + ex.Message);
            }
        }

        report.AppendLine();
        report.AppendLine("Cierre: objetivo procesado por i@N Auto.");
        File.WriteAllText(log, report.ToString(), Encoding.UTF8);
        Console.WriteLine("Registro: " + log);
        return 0;
    }

    static string RunCapture(string exe, string args, StringBuilder report)
    {
        string path = Path.Combine(Home, exe);
        var psi = new ProcessStartInfo();
        psi.FileName = path;
        psi.Arguments = args;
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.WorkingDirectory = Home;

        using (var p = Process.Start(psi))
        {
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            report.AppendLine("> " + exe + " " + args);
            report.AppendLine(output);
            if (!string.IsNullOrWhiteSpace(error)) report.AppendLine(error);
            if (p.ExitCode != 0) throw new Exception(error.Trim().Length > 0 ? error.Trim() : output.Trim());
            return output;
        }
    }

    static string ExtractGeneratedPath(string text)
    {
        using (var reader = new StringReader(text ?? ""))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                const string prefix = "Archivo creado:";
                if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return line.Substring(prefix.Length).Trim();
            }
        }
        return "";
    }

    static string Q(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
    }

    static string[] Slice(string[] input, int start)
    {
        if (start >= input.Length) return new string[0];
        string[] output = new string[input.Length - start];
        for (int i = start; i < input.Length; i++) output[i - start] = input[i];
        return output;
    }
}

