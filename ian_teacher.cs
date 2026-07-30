using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

class IanTeacher
{
    const string Version = "0.1.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string Inbox = Path.Combine(Home, "teach-inbox");
    static readonly string CodexDir = Path.Combine(Inbox, "codex");
    static readonly string ClaudeDir = Path.Combine(Inbox, "claude");
    static readonly string DeepSeekDir = Path.Combine(Inbox, "deepseek");
    static readonly string ManualDir = Path.Combine(Inbox, "manual");
    static readonly string LearnedDir = Path.Combine(Inbox, "aprendido");
    static readonly string BrainDir = Path.Combine(Home, "brain");
    static readonly string MemoryFile = Path.Combine(BrainDir, "memory.tsv");

    public static int Run(string[] args)
    {
        Ensure();

        if (args.Length == 0 || args[0] == "--help")
        {
            Help();
            return 0;
        }

        try
        {
            string cmd = args[0].ToLowerInvariant();
            if (cmd == "--version")
            {
                Console.WriteLine("i@N Teacher " + Version);
                return 0;
            }
            if (cmd == "estado") return Status();
            if (cmd == "plantillas") return Templates();
            if (cmd == "importar") return ImportAll();
            if (cmd == "pedir") return AskTeacher(args);
            if (cmd == "vigilar") return Watch(args);
            if (cmd == "recibir") return Receive(args);
            if (cmd == "abrir") return OpenPanel();
            throw new Exception("Comando desconocido: " + args[0]);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N Teacher error: " + ex.Message);
            return 1;
        }
    }

    static void Help()
    {
        Console.WriteLine("i@N Teacher " + Version);
        Console.WriteLine("Uso:");
        Console.WriteLine("  ian.exe teacher estado");
        Console.WriteLine("  ian.exe teacher plantillas");
        Console.WriteLine("  ian.exe teacher importar");
        Console.WriteLine("  ian.exe teacher pedir claude \"pedido\"");
        Console.WriteLine("  ian.exe teacher pedir codex \"pedido\"");
        Console.WriteLine("  ian.exe teacher pedir deepseek \"pedido\"");
        Console.WriteLine("  ian.exe teacher vigilar");
        Console.WriteLine("  ian.exe teacher recibir claude \"pedido\" \"archivo.ian\"");
        Console.WriteLine("  ian.exe teacher recibir codex \"pedido\" \"archivo.ian\"");
        Console.WriteLine("  ian.exe teacher abrir");
    }

    static void Ensure()
    {
        Directory.CreateDirectory(CodexDir);
        Directory.CreateDirectory(ClaudeDir);
        Directory.CreateDirectory(DeepSeekDir);
        Directory.CreateDirectory(ManualDir);
        Directory.CreateDirectory(LearnedDir);
        Directory.CreateDirectory(BrainDir);
    }

    static int Status()
    {
        Console.WriteLine("Bandeja Codex: " + CountFiles(CodexDir) + " archivos");
        Console.WriteLine("Bandeja Claude: " + CountFiles(ClaudeDir) + " archivos");
        Console.WriteLine("Bandeja DeepSeek: " + CountFiles(DeepSeekDir) + " archivos");
        Console.WriteLine("Bandeja Manual: " + CountFiles(ManualDir) + " archivos");
        Console.WriteLine("Memorias Brain: " + (File.Exists(MemoryFile) ? File.ReadAllLines(MemoryFile, Encoding.UTF8).Length : 0));
        Console.WriteLine("Panel: " + Path.Combine(Home, "TEACHER_PANEL.html"));
        return 0;
    }

    static int Templates()
    {
        WriteTemplate("PROMPT_CODEX.txt", "Codex");
        WriteTemplate("PROMPT_CLAUDE.txt", "Claude");
        WriteTemplate("PROMPT_DEEPSEEK.txt", "DeepSeek");
        Console.WriteLine("Plantillas creadas en: " + Inbox);
        return 0;
    }

    static int ImportAll()
    {
        int count = 0;
        count += ImportDir(CodexDir, "codex");
        count += ImportDir(ClaudeDir, "claude");
        count += ImportDir(DeepSeekDir, "deepseek");
        count += ImportDir(ManualDir, "manual");
        Console.WriteLine("Ense??anzas importadas: " + count);
        return 0;
    }

    static int AskTeacher(string[] args)
    {
        if (args.Length < 3) throw new Exception("Uso: pedir claude|codex \"pedido\"");
        string teacher = args[1].ToLowerInvariant();
        string request = string.Join(" ", Slice(args, 2));
        if (teacher != "claude" && teacher != "codex" && teacher != "deepseek") throw new Exception("Profesor no soportado: " + teacher);

        string prompt = BuildTeachingPrompt(request, teacher);
        Console.WriteLine("Pidiendo ense??anza a " + teacher + "...");

        string output = RunProvider(teacher, prompt);
        string code = CleanProviderOutput(output);
        if (string.IsNullOrWhiteSpace(code)) throw new Exception(teacher + " no devolvio una ense??anza util");
        if (!code.TrimStart().StartsWith("# pregunta:", StringComparison.OrdinalIgnoreCase))
            code = "# pregunta: " + request + Environment.NewLine + code.Trim();

        string dir = teacher == "claude" ? ClaudeDir : (teacher == "deepseek" ? DeepSeekDir : CodexDir);
        string file = Path.Combine(dir, "auto_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".teach");
        File.WriteAllText(file, code, Encoding.UTF8);
        Console.WriteLine("Ense??anza guardada: " + file);
        return ImportAll();
    }

    static int Watch(string[] args)
    {
        int seconds = 5;
        if (args.Length >= 2) int.TryParse(args[1], out seconds);
        if (seconds < 1) seconds = 5;
        Console.WriteLine("i@N Teacher vigilando bandejas cada " + seconds + " segundos.");
        Console.WriteLine("Para detener: Ctrl+C");
        while (true)
        {
            int count = 0;
            count += ImportDir(CodexDir, "codex");
            count += ImportDir(ClaudeDir, "claude");
            count += ImportDir(DeepSeekDir, "deepseek");
            count += ImportDir(ManualDir, "manual");
            if (count > 0) Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " - ense??anzas importadas: " + count);
            System.Threading.Thread.Sleep(seconds * 1000);
        }
    }

    static int Receive(string[] args)
    {
        if (args.Length < 4) throw new Exception("Uso: recibir claude|codex|manual \"pedido\" \"archivo.ian\"");
        string teacher = args[1].ToLowerInvariant();
        string prompt = args[2];
        string file = Path.GetFullPath(args[3]);
        if (!File.Exists(file)) throw new Exception("No existe el archivo: " + file);
        string code = File.ReadAllText(file, Encoding.UTF8).Trim();
        Learn(teacher, prompt, code);
        Console.WriteLine("Ense??anza recibida de " + teacher + ".");
        return 0;
    }

    static int OpenPanel()
    {
        string panel = Path.Combine(Home, "TEACHER_PANEL.html");
        if (!File.Exists(panel)) throw new Exception("No existe TEACHER_PANEL.html");
        Process.Start(panel);
        return 0;
    }

    static string BuildTeachingPrompt(string request, string teacher)
    {
        return
            "Quiero que le ense??es a i@N Brain a resolver este pedido: " + request + "\n\n" +
            "Responde SOLO con un archivo de ense??anza i@N. No uses Markdown. No expliques.\n" +
            "Formato obligatorio:\n\n" +
            "# pregunta: " + request + "\n" +
            "CODIGO i@N ACA\n\n" +
            "Comandos disponibles (i@N v0.3):\n" +
            "decir \"texto\"\n" +
            "guardar nombre = \"valor\"\n" +
            "sumar $a $b como resultado\n" +
            "restar $a $b como resultado\n" +
            "multiplicar $a $b como resultado\n" +
            "dividir $a $b como resultado\n" +
            "modulo $a $b como resultado\n" +
            "incrementar nombre\n" +
            "decrementar nombre\n" +
            "texto longitud $var como largo\n" +
            "texto mayusculas $var como upper\n" +
            "texto minusculas $var como lower\n" +
            "texto contiene $var \"cadena\" como encontrado\n" +
            "texto recortar $var como limpio\n" +
            "unir \"Hola\" \" \" $nombre como resultado\n" +
            "lista crear como items\n" +
            "lista agregar items \"valor\"\n" +
            "lista obtener items 0 como primero\n" +
            "lista tamanno items como cantidad\n" +
            "lista json items como json_items\n" +
            "si $var mayor que 0 entonces\n" +
            "  decir \"positivo\"\n" +
            "sino\n" +
            "  decir \"no positivo\"\n" +
            "fin\n" +
            "mientras $i menor que 10\n" +
            "  incrementar i\n" +
            "fin\n" +
            "repetir 3 veces\n" +
            "  decir \"iterando\"\n" +
            "fin\n" +
            "red dns \"dominio\" como resultado\n" +
            "red puerto \"host\" 80 como resultado\n" +
            "api get \"https://url\" como respuesta\n" +
            "web iniciar \"Titulo\" como pagina\n" +
            "web titulo pagina \"Titulo\"\n" +
            "web texto pagina \"Texto\"\n" +
            "web boton pagina \"Etiqueta\" mensaje \"Mensaje\"\n" +
            "web guardar pagina en \"salida/index.html\"\n" +
            "mikrotik script \"/interface print\" como resultado\n" +
            "agente plan \"Objetivo\" como plan\n" +
            "agente paso plan hacer \"Accion\"\n" +
            "agente json plan como resumen\n" +
            "decir $resumen\n\n" +
            "Operadores para si/mientras: es, no es, mayor que, menor que, mayor o igual que, menor o igual que, contiene\n" +
            "archivo leer \"ruta\" como contenido\n" +
            "archivo escribir \"ruta\" contenido\n" +
            "archivo agregar \"ruta\" contenido\n" +
            "si archivo existe \"ruta\" entonces ... fin\n" +
            "incluir \"otro.ian\"\n\n" +
            "Profesor: " + teacher;
    }

    static string RunProvider(string teacher, string prompt)
    {
        string command;
        if (teacher == "codex")
        {
            command = "codex exec --skip-git-repo-check --sandbox read-only " + QuoteArg(prompt);
        }
        else if (teacher == "deepseek")
        {
            command = "deepseek " + QuoteArg(prompt);
        }
        else
        {
            command = "claude -p " + QuoteArg(prompt);
        }

        var psi = new ProcessStartInfo();
        psi.FileName = "cmd.exe";
        psi.Arguments = "/c " + command;
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.WorkingDirectory = Home;

        using (var process = Process.Start(psi))
        {
            string output = "";
            string error = "";
            var stdoutThread = new System.Threading.Thread(() => { try { output = process.StandardOutput.ReadToEnd(); } catch {} });
            var stderrThread = new System.Threading.Thread(() => { try { error = process.StandardError.ReadToEnd(); } catch {} });
            stdoutThread.Start();
            stderrThread.Start();
            if (!process.WaitForExit(180000))
            {
                try { process.Kill(); } catch { }
                throw new Exception(teacher + " tardo demasiado en responder");
            }
            stdoutThread.Join();
            stderrThread.Join();
            if (process.ExitCode != 0)
                throw new Exception((error.Trim().Length > 0 ? error.Trim() : output.Trim()));
            return output;
        }
    }

    static string CleanProviderOutput(string output)
    {
        string text = (output ?? "").Trim();
        string learned = ExtractIanBrainLearnCommand(text);
        if (!string.IsNullOrWhiteSpace(learned)) return learned;
        if (text.StartsWith("```"))
        {
            int firstBreak = text.IndexOf('\n');
            int lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (firstBreak >= 0 && lastFence > firstBreak)
                text = text.Substring(firstBreak + 1, lastFence - firstBreak - 1).Trim();
        }
        return text;
    }

    static string ExtractIanBrainLearnCommand(string text)
    {
        int marker = text.IndexOf("ian.exe brain aprender", StringComparison.OrdinalIgnoreCase);
        if (marker < 0) marker = text.IndexOf("ian-brain.exe aprender", StringComparison.OrdinalIgnoreCase);
        if (marker < 0) marker = text.IndexOf("ian-brain aprender", StringComparison.OrdinalIgnoreCase);
        if (marker < 0) return "";

        string tail = text.Substring(marker);
        int aprender = tail.IndexOf("aprender", StringComparison.OrdinalIgnoreCase);
        if (aprender < 0) return "";
        tail = tail.Substring(aprender + "aprender".Length);

        int pos = 0;
        string prompt = ReadQuoted(tail, ref pos);
        string code = ReadQuoted(tail, ref pos);
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(code)) return "";

        code = code.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("`\"", "\"").Trim();
        return "# pregunta: " + prompt.Trim() + Environment.NewLine + code;
    }

    static string ReadQuoted(string text, ref int pos)
    {
        while (pos < text.Length && text[pos] != '"') pos++;
        if (pos >= text.Length) return "";
        pos++;
        var builder = new StringBuilder();
        bool backslash = false;
        bool backtick = false;
        while (pos < text.Length)
        {
            char c = text[pos++];
            if (backslash)
            {
                if (c == 'n') builder.Append('\n');
                else if (c == 'r') builder.Append('\r');
                else if (c == 't') builder.Append('\t');
                else builder.Append(c);
                backslash = false;
                continue;
            }
            if (backtick)
            {
                builder.Append(c);
                backtick = false;
                continue;
            }
            if (c == '\\') { backslash = true; continue; }
            if (c == '`') { backtick = true; continue; }
            if (c == '"') break;
            builder.Append(c);
        }
        return builder.ToString();
    }

    static int ImportDir(string dir, string teacher)
    {
        int count = 0;
        foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext != ".teach" && ext != ".ian") continue;
            Teaching t = ReadTeaching(file);
            Learn(teacher, t.Prompt, t.Code);
            string target = Path.Combine(LearnedDir, DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Path.GetFileName(file));
            if (File.Exists(target)) target = target + ".old";
            File.Move(file, target);
            count++;
        }
        return count;
    }

    static Teaching ReadTeaching(string file)
    {
        string text = File.ReadAllText(file, Encoding.UTF8).Trim();
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        string prompt = Path.GetFileNameWithoutExtension(file).Replace("_", " ").Replace("-", " ");
        int start = 0;
        if (lines.Length > 0 && lines[0].StartsWith("# pregunta:", StringComparison.OrdinalIgnoreCase))
        {
            prompt = lines[0].Substring(lines[0].IndexOf(':') + 1).Trim();
            start = 1;
        }
        var code = new StringBuilder();
        for (int i = start; i < lines.Length; i++) code.AppendLine(lines[i]);
        return new Teaching(prompt, code.ToString().Trim());
    }

    static void Learn(string teacher, string prompt, string code)
    {
        if (string.IsNullOrWhiteSpace(prompt)) throw new Exception("La ense??anza no tiene pedido/pregunta");
        if (string.IsNullOrWhiteSpace(code)) throw new Exception("La ense??anza no tiene codigo i@N");
        File.AppendAllText(
            MemoryFile,
            Escape(prompt) + "\t" + Escape(code) + "\t" + DateTime.Now.ToString("s") + "\t" + Escape(teacher) + Environment.NewLine,
            Encoding.UTF8
        );
    }

    static void WriteTemplate(string name, string teacher)
    {
        string text =
            "Quiero que le ense??es a i@N Brain.\n" +
            "i@N v0.3 es un lenguaje local con estos comandos:\n" +
            "- decir \"texto\" / decir $variable\n" +
            "- guardar nombre = \"valor\"\n" +
            "- sumar/restar/multiplicar/dividir/modulo $a $b como resultado\n" +
            "- incrementar nombre / decrementar nombre\n" +
            "- texto longitud/mayusculas/minusculas/contiene/recortar $var ...\n" +
            "- unir \"parte1\" $var \"parte2\" como resultado\n" +
            "- lista crear/agregar/obtener/tamanno/json ...\n" +
            "- si $var OPERADOR valor entonces ... sino ... fin\n" +
            "- mientras $var OPERADOR valor ... fin\n" +
            "- repetir N veces ... fin\n" +
            "- red dns \"dominio\" como resultado\n" +
            "- red puerto \"host\" 80 como resultado\n" +
            "- api get \"https://url\" como respuesta\n" +
            "- web iniciar \"Titulo\" como pagina\n" +
            "- web titulo/texto/boton/guardar ...\n" +
            "- mikrotik script \"/comando\" como resultado\n" +
            "- agente plan \"Objetivo\" como plan\n" +
            "- agente paso plan hacer/haciendo/hecho \"Accion\"\n" +
            "- agente json plan como resumen\n" +
            "Operadores: es, no es, mayor que, menor que, mayor o igual que, menor o igual que, contiene\n\n" +
            "Respondeme con un archivo de ense??anza en este formato exacto:\n\n" +
            "# pregunta: ESCRIBIR ACA EL PEDIDO QUE DEBE APRENDER i@N\n" +
            "CODIGO i@N ACA\n\n" +
            "No expliques nada fuera del codigo. No uses Markdown. No uses Python.\n" +
            "Profesor sugerido: " + teacher + "\n";
        File.WriteAllText(Path.Combine(Inbox, name), text, Encoding.UTF8);
    }

    static int CountFiles(string dir)
    {
        int count = 0;
        foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext == ".teach" || ext == ".ian") count++;
        }
        return count;
    }

    static string Escape(string text)
    {
        return (text ?? "").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    static string QuoteArg(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
    }

    static string[] Slice(string[] input, int start)
    {
        var output = new string[input.Length - start];
        for (int i = start; i < input.Length; i++) output[i - start] = input[i];
        return output;
    }
}

struct Teaching
{
    public string Prompt;
    public string Code;
    public Teaching(string prompt, string code)
    {
        Prompt = prompt;
        Code = code;
    }
}

