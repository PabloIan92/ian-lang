using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

class IanBrain
{
    const string Version = "0.5.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string BrainDir = Path.Combine(Home, "brain");
    static readonly string MemoryFile = Path.Combine(BrainDir, "memory.tsv");
    static readonly string KnowledgeFile = Path.Combine(BrainDir, "knowledge.txt");
    static readonly string ContextDir = Path.Combine(Home, "context");
    static readonly string ContextFile = Path.Combine(Home, "context", "session.tsv");

    public static int Run(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding  = System.Text.Encoding.UTF8;
        Directory.CreateDirectory(BrainDir);
        if (args.Length == 0 || args[0] == "--help") { Help(); return 0; }
        try
        {
            string cmd = args[0].ToLowerInvariant();
            if (cmd == "--version") { Console.WriteLine("i@N Brain " + Version); return 0; }
            if (cmd == "aprender")        return Learn(args);
            if (cmd == "aprender-archivo") return LearnFile(args);
            if (cmd == "preguntar")       return Ask(args, false);
            if (cmd == "crear")           return Ask(args, true);
            if (cmd == "ejecutar")        return Ask(args, true, true);
            if (cmd == "importar")        return Import(args);
            if (cmd == "estado")          return Status();
            if (cmd == "vision")          return Vision();
            if (cmd == "chat")            return Chat();
            throw new Exception("Comando desconocido: " + args[0]);
        }
        catch (Exception ex) { Console.Error.WriteLine("i@N Brain error: " + ex.Message); return 1; }
    }

    static void Help()
    {
        Console.WriteLine("i@N Brain " + Version);
        Console.WriteLine("Uso: aprender | preguntar | crear | ejecutar | importar | estado | vision | chat");
    }

    // -------------------------------------------------------------------------
    //  LEARN
    // -------------------------------------------------------------------------

    static int Learn(string[] args)
    {
        if (args.Length < 3) throw new Exception("Uso: aprender \"pregunta\" \"programa i@N\"");
        string prompt  = args[1].Trim();
        string program = args[2].Replace("\\n", "\n").Trim();
        AppendMemory(prompt, program, "manual");
        Console.WriteLine("Aprendido.");
        return 0;
    }

    static int LearnFile(string[] args)
    {
        if (args.Length < 3) throw new Exception("Uso: aprender-archivo \"pregunta\" \"archivo.ian\"");
        string prompt = args[1].Trim();
        string path   = Path.GetFullPath(args[2]);
        if (!File.Exists(path)) throw new Exception("No existe el archivo: " + path);
        string program = File.ReadAllText(path, Encoding.UTF8).Trim();
        AppendMemory(prompt, program, "archivo");
        Console.WriteLine("Aprendido desde archivo.");
        return 0;
    }

    static void AppendMemory(string prompt, string program, string source)
    {
        File.AppendAllText(
            MemoryFile,
            Escape(prompt) + "\t" + Escape(program) + "\t" + DateTime.Now.ToString("s") + "\t" + source + Environment.NewLine,
            Encoding.UTF8
        );
    }

    // -------------------------------------------------------------------------
    //  ASK / EXECUTE
    // -------------------------------------------------------------------------

    static int Ask(string[] args, bool create, bool runAfter = false)
    {
        if (args.Length < 2) throw new Exception((create ? "crear" : "preguntar") + " necesita un pedido");
        string prompt = string.Join(" ", Slice(args, 1));

        SaveContext(prompt);

        string program = Think(prompt);
        if (!runAfter) Console.WriteLine(program);
        if (create)
        {
            string generated = Path.Combine(Home, "generated");
            Directory.CreateDirectory(generated);
            string file = Path.Combine(generated, "brain_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".ian");
            File.WriteAllText(file, program, Encoding.UTF8);
            if (runAfter)
            {
                var psi = new ProcessStartInfo(Path.Combine(Home, "ian.exe"), "--allow-run \"" + file + "\"");
                psi.UseShellExecute = false;
                psi.WorkingDirectory = Path.GetDirectoryName(file);
                using (var p = Process.Start(psi)) { p.WaitForExit(); return p.ExitCode; }
            }
        }
        return 0;
    }

    // -------------------------------------------------------------------------
    //  IMPORT
    // -------------------------------------------------------------------------

    static int Import(string[] args)
    {
        if (args.Length < 2) throw new Exception("Uso: importar \"carpeta\"");
        string dir = Path.GetFullPath(args[1]);
        if (!Directory.Exists(dir)) throw new Exception("No existe la carpeta: " + dir);
        int files = 0;
        foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext != ".txt" && ext != ".md" && ext != ".ian") continue;
            File.AppendAllText(KnowledgeFile, "\n\n--- archivo: " + file + " ---\n" + File.ReadAllText(file, Encoding.UTF8), Encoding.UTF8);
            files++;
        }
        Console.WriteLine("Importados " + files + " archivos.");
        return 0;
    }

    // -------------------------------------------------------------------------
    //  STATUS / VISION / CHAT
    // -------------------------------------------------------------------------

    static int Status()
    {
        int memories = File.Exists(MemoryFile) ? File.ReadAllLines(MemoryFile, Encoding.UTF8).Length : 0;
        int ctx      = File.Exists(ContextFile) ? File.ReadAllLines(ContextFile, Encoding.UTF8).Length : 0;
        Console.WriteLine("i@N Brain v" + Version);
        Console.WriteLine("Memorias guardadas : " + memories);
        Console.WriteLine("Contexto de sesion : " + ctx + " turnos recientes");
        return 0;
    }

    static int Vision()
    {
        int memories = File.Exists(MemoryFile) ? File.ReadAllLines(MemoryFile, Encoding.UTF8).Length : 0;
        bool hasDeepSeek = File.Exists(Path.Combine(Home, "deepseek.py")) &&
                           !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? "");

        Console.WriteLine("Vision de i@N v" + Version + " ??? IA local autonoma");
        Console.WriteLine();
        Console.WriteLine("Modulos activos:");
        Console.WriteLine("  Motor      ian.exe              interpreta programas .ian");
        Console.WriteLine("  Cerebro    ian.exe brain         memoria, razonamiento, contexto");
        Console.WriteLine("  Agente     ian.exe agent         convierte pedidos a programas");
        Console.WriteLine("  Profesor   ian.exe teacher       aprende de Claude, Codex y DeepSeek");
        Console.WriteLine("  Supervisor ian.exe supervisor    revisa calidad y seguridad");
        Console.WriteLine("  Autonomia  ian.exe auto          actua por objetivos con logs");
        Console.WriteLine("  API        ian.exe api           conecta fuentes externas");
        Console.WriteLine("  Arquitecto ian.exe architect     mapea la evolucion interna");
        Console.WriteLine();
        Console.WriteLine("Cadena de conocimiento (omnisciencia operativa):");
        Console.WriteLine("  1. Memoria local (" + memories + " entradas)");
        Console.WriteLine("  2. Patrones conocidos (identidad, red, web, Mikrotik...)");
        Console.WriteLine("  3. Memoria relajada (coincidencia parcial)");
        Console.WriteLine("  4. DeepSeek ??? " + (hasDeepSeek ? "ACTIVO" : "inactivo (falta DEEPSEEK_API_KEY)"));
        Console.WriteLine("  5. Claude   ??? activo (fallback final)");
        Console.WriteLine();
        Console.WriteLine("Proximos pasos:");
        Console.WriteLine("  1. Clasificar intencion antes de ejecutar (conversacion vs accion)");
        Console.WriteLine("  2. Puntaje de calidad por memoria (no solo frecuencia de match)");
        Console.WriteLine("  3. Aprender de cada interaccion, no solo de ensenanzas manuales");
        Console.WriteLine("  4. Conexion SSH real con Mikrotik RouterOS");
        Console.WriteLine("  5. Transpilador i@N -> JavaScript");
        return 0;
    }

    static int Chat()
    {
        Console.WriteLine("i@N Brain v" + Version + " ??? Modo conversacional. 'salir' para terminar.");
        while (true)
        {
            Console.Write("i@N> ");
            string line = Console.ReadLine();
            if (line == null) return 0;
            string lower = line.Trim().ToLowerInvariant();
            if (lower == "salir" || lower == "exit" || lower == "chau") return 0;
            SaveContext(line);
            string program = Think(line);
            // In chat mode, execute the generated program directly
            string tmpDir = Path.Combine(Home, "generated");
            Directory.CreateDirectory(tmpDir);
            string tmp = Path.Combine(tmpDir, "brain_chat_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".ian");
            File.WriteAllText(tmp, program, Encoding.UTF8);
            var psi = new ProcessStartInfo(Path.Combine(Home, "ian.exe"), "--allow-run \"" + tmp + "\"");
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Home;
            using (var p = Process.Start(psi)) { p.WaitForExit(); }
        }
    }

    // -------------------------------------------------------------------------
    //  CONTEXT (session memory)
    // -------------------------------------------------------------------------

    static void SaveContext(string prompt)
    {
        try
        {
            Directory.CreateDirectory(ContextDir);
            var lines = new List<string>();
            if (File.Exists(ContextFile))
                lines.AddRange(File.ReadAllLines(ContextFile, Encoding.UTF8));
            lines.Add(DateTime.Now.ToString("s") + "\t" + Escape(prompt));
            if (lines.Count > 10) lines = lines.GetRange(lines.Count - 10, 10);
            File.WriteAllLines(ContextFile, lines.ToArray(), Encoding.UTF8);
        }
        catch { }
    }

    static List<string> GetRecentContext(int count)
    {
        var result = new List<string>();
        if (!File.Exists(ContextFile)) return result;
        try
        {
            string[] lines = File.ReadAllLines(ContextFile, Encoding.UTF8);
            int start = Math.Max(0, lines.Length - count);
            for (int i = start; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('\t');
                if (parts.Length >= 2) result.Add(Unescape(parts[1]));
            }
        }
        catch { }
        return result;
    }

    // -------------------------------------------------------------------------
    //  THINK ENGINE ??? sin patrones hardcodeados
    //  Flujo: memoria confiable ??? DeepSeek ??? Claude ??? fallback
    // -------------------------------------------------------------------------

    static string Think(string prompt)
    {
        string[] pw = Words(prompt);

        // Memoria: requiere al menos 60% de cobertura Y 2+ palabras coincidentes
        // Esto evita falsos matches en preguntas cortas o con pocas palabras comunes
        MemoryMatch match = BestMemory(prompt, 0.60f);
        if (match.Score >= 2) return match.Program;

        // Para preguntas de una sola palabra significativa (hola, gracias, etc.)
        // un match perfecto (100%) con score=1 es v??lido
        if (match.Score == 1 && pw.Length == 1) return match.Program;

        // Sin memoria confiable ??? DeepSeek (si tiene API key) ??? Claude
        string ds = AskDeepSeekAsIan(prompt);
        if (ds != null) return ds;

        return AskClaudeAsIan(prompt);
    }

    // -------------------------------------------------------------------------
    //  CLAUDE FALLBACK ??? sin limite de consultas
    // -------------------------------------------------------------------------

    static string AskClaudeAsIan(string prompt)
    {
        try
        {
            string domain     = ClassifyDomain(Words(prompt));
            string sessionCtx = BuildSessionContext(4);
            string memoryCtx  = BuildMemoryContext(prompt, 5);
            string sysPrompt  = BuildSystemPrompt(prompt, domain, sessionCtx, memoryCtx);
            // Flatten to single line ??? embedded newlines break Windows CreateProcess arg parsing
            string sysLine    = sysPrompt.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

            var psi = new ProcessStartInfo();
            psi.FileName = "claude";
            psi.Arguments = "-p \"" + EscapeForIan(sysLine) + "\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = Home;

            string output = "";
            using (var p = Process.Start(psi))
            {
                var t1 = new System.Threading.Thread(() => { try { output = p.StandardOutput.ReadToEnd(); } catch { } });
                t1.Start();
                bool ok = p.WaitForExit(30000);
                t1.Join(5000);
                if (!ok || p.ExitCode != 0) return FallbackNoAnswer(prompt);
            }

            string text = (output ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return FallbackNoAnswer(prompt);

            // Detect i@N program block ??? Claude generates code between markers
            const string IAN_START = "---IAN---";
            const string IAN_END   = "---FIN---";
            int iStart = text.IndexOf(IAN_START);
            int iEnd   = iStart >= 0 ? text.IndexOf(IAN_END, iStart + IAN_START.Length) : -1;

            if (iStart >= 0 && iEnd > iStart)
            {
                string ianCode = text.Substring(iStart + IAN_START.Length, iEnd - iStart - IAN_START.Length).Trim();
                AppendMemory(prompt, ianCode, "claude-auto");
                return ianCode;
            }

            // Plain conversational text ??? strip markdown fences if present
            if (text.StartsWith("```"))
            {
                int nl = text.IndexOf('\n');
                int fence = text.LastIndexOf("```");
                if (nl >= 0 && fence > nl)
                    text = text.Substring(nl + 1, fence - nl - 1).Trim();
            }

            var code = new StringBuilder();
            code.AppendLine("# i@N Brain v" + Version + " (via Claude/" + domain + ")");
            foreach (string line in text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string l = line.Trim();
                if (l.Length > 0)
                    code.AppendLine("ia decir \"" + EscapeForIan(l) + "\"");
            }

            AppendMemory(prompt, code.ToString().Trim(), "claude-auto");
            return code.ToString();
        }
        catch
        {
            return FallbackNoAnswer(prompt);
        }
    }

    // ??ltimas N interacciones del usuario en esta sesi??n
    static string BuildSessionContext(int count)
    {
        var turns = GetRecentContext(count);
        if (turns.Count == 0) return "";
        var sb = new StringBuilder();
        sb.Append("Historial de esta sesion:");
        foreach (string t in turns)
            sb.Append(" | " + t);
        return sb.ToString();
    }

    // Top N entradas de memoria relacionadas con el prompt actual
    static string BuildMemoryContext(string prompt, int topN)
    {
        if (!File.Exists(MemoryFile)) return "";
        string[] pw = Words(prompt);
        if (pw.Length == 0) return "";

        var scored = new List<KeyValuePair<int, string>>();
        foreach (string line in File.ReadAllLines(MemoryFile, Encoding.UTF8))
        {
            string[] parts = line.Split('\t');
            if (parts.Length < 2) continue;
            string key = Unescape(parts[0]);
            int sc = Score(pw, Words(key));
            if (sc >= 1)
                scored.Add(new KeyValuePair<int, string>(sc, key));
        }
        if (scored.Count == 0) return "";

        scored.Sort((a, b) => b.Key.CompareTo(a.Key));
        if (scored.Count > topN) scored = scored.GetRange(0, topN);

        var sb = new StringBuilder();
        sb.Append("Temas relacionados en mi memoria:");
        foreach (var kv in scored)
            sb.Append(" | " + kv.Value);
        return sb.ToString();
    }

    // Prompt completo para Claude ??? identidad, contexto, sintaxis i@N
    static string BuildSystemPrompt(string prompt, string domain, string sessionCtx, string memoryCtx)
    {
        var sb = new StringBuilder();
        sb.Append("Sos i@N, una IA local que corre en la PC de tu creador en Argentina. ");
        sb.Append("Tu creador es empresario de construccion y administra redes Mikrotik. ");
        sb.Append("Respond??s en espa??ol rioplatense (vos, che), directo y sin formalismos. ");
        sb.Append("No sos Claude ni lo menciones. Ten??s identidad propia como i@N. ");
        sb.Append("No uses markdown, asteriscos ni guiones al inicio de linea. ");
        if (!string.IsNullOrEmpty(sessionCtx)) sb.Append(sessionCtx + ". ");
        if (!string.IsNullOrEmpty(memoryCtx))  sb.Append(memoryCtx + ". ");
        sb.Append("Dominio detectado: " + domain + ". ");
        sb.Append("Cuando el pedido es una accion concreta (crear web, Excel, Word, PDF, guardar archivo, llamar API), ");
        sb.Append("genera un programa i@N y respond?? SOLO con el bloque entre ---IAN--- y ---FIN--- sin ningun texto adicional. ");
        sb.Append("Comandos i@N disponibles: ");
        sb.Append("ia decir \"texto\" (muestra texto al usuario) ??? ");
        sb.Append("ia guardar \"nombre.txt\" \"contenido\" (REGLAS CRITICAS: 1) usa SOLO el nombre del archivo, NUNCA ruta completa ni backslashes; 2) todo el contenido en UNA SOLA LINEA entre comillas; 3) usa \\n para saltos de linea dentro de las comillas; EJEMPLO CORRECTO: ia guardar \"tareas.txt\" \"Item 1\\nItem 2\\nItem 3\"; EJEMPLO INCORRECTO: ia guardar \"C:\\Users\\...\" texto sin comillas en varias lineas) ??? ");
        sb.Append("web crear \"pag\" | web titulo \"pag\" \"t\" | web subtitulo \"pag\" \"t\" | web parrafo \"pag\" \"t\" | web lista \"pag\" \"a\" \"b\" | web guardar \"pag\" en \"x.html\" | web pdf \"pag\" en \"x.pdf\" ??? ");
        sb.Append("excel crear \"hoja\" | excel fila \"hoja\" \"A\" \"B\" \"C\" | excel guardar \"hoja\" en \"x.xls\" ??? ");
        sb.Append("word crear \"doc\" | word titulo \"doc\" \"t\" | word parrafo \"doc\" \"t\" | word guardar \"doc\" en \"x.doc\" ??? ");
        sb.Append("api get \"url\" como var | api post \"url\" \"json\" como var. ");
        sb.Append("Cuando el pedido es conversacional (pregunta, consejo, explicacion, charla), ");
        sb.Append("respond?? en texto directo sin el bloque ---IAN---. Maximo 5 oraciones. ");
        sb.Append("Pedido: " + prompt);
        return sb.ToString();
    }

    static string FallbackNoAnswer(string prompt)
    {
        var code = new StringBuilder();
        code.AppendLine("# i@N Brain v" + Version);
        code.AppendLine("ia decir \"No tengo informacion especifica sobre eso todavia.\"");
        code.AppendLine("ia decir \"Para ensenarme: ian-teacher pedir claude o ian-teacher pedir deepseek\"");
        return code.ToString();
    }

    static string BuildVisionProgram()
    {
        var code = new StringBuilder();
        code.AppendLine("# vision i@N");
        code.AppendLine("ia decir \"Vision: ser una IA local, autonoma y util.\"");
        code.AppendLine("ia decir \"Modulos: motor, cerebro, agente, profesor, supervisor, autonomia.\"");
        code.AppendLine("ia decir \"Proximo paso: clasificar intencion antes de ejecutar.\"");
        code.AppendLine("ia decir \"Mas info: escribi 'vision' en la consola.\"");
        return code.ToString();
    }

    // -------------------------------------------------------------------------
    //  DEEPSEEK FALLBACK ??? segundo profesor externo
    // -------------------------------------------------------------------------

    static string AskDeepSeekAsIan(string prompt)
    {
        try
        {
            string deepseekPy = Path.Combine(Home, "deepseek.py");
            if (!File.Exists(deepseekPy)) return null;

            string apiKey = (Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? "").Trim();
            if (string.IsNullOrEmpty(apiKey)) return null;

            string domain = ClassifyDomain(Words(prompt));
            string fullPrompt =
                "Sos i@N, una IA local que corre en esta PC. " +
                "Responde en espa??ol rioplatense, directo y sin formalismos. " +
                "Dominio: " + domain + ". " +
                "Maximo 4 oraciones. Sin markdown ni asteriscos. " +
                "Pregunta: " + prompt;

            var psi = new ProcessStartInfo();
            psi.FileName = "python";
            psi.Arguments = "\"" + deepseekPy + "\" \"" + EscapeForIan(fullPrompt) + "\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = Home;

            string output = "";
            using (var p = Process.Start(psi))
            {
                var t1 = new System.Threading.Thread(() => { try { output = p.StandardOutput.ReadToEnd(); } catch { } });
                t1.Start();
                bool ok = p.WaitForExit(30000);
                t1.Join(5000);
                if (!ok || p.ExitCode != 0) return null;
            }

            string text = (output ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var code = new StringBuilder();
            code.AppendLine("# i@N Brain v" + Version + " (via DeepSeek/" + domain + ")");
            string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string l = line.Trim();
                if (l.Length > 0)
                    code.AppendLine("ia decir \"" + EscapeForIan(l) + "\"");
            }

            AppendMemory(prompt, code.ToString().Trim(), "deepseek-auto");
            return code.ToString();
        }
        catch
        {
            return null;
        }
    }

    // Clasificar dominio del prompt para enriquecer contexto del fallback
    static string ClassifyDomain(string[] words)
    {
        if (Has(words, "red") || Has(words, "dns") || Has(words, "ip") || Has(words, "puerto") || Has(words, "firewall") || Has(words, "ping") || Has(words, "subred") || Has(words, "vlan")) return "red";
        if (Has(words, "mikrotik") || Has(words, "routeros") || Has(words, "router") || Has(words, "winbox") || Has(words, "ospf") || Has(words, "bgp") || Has(words, "queue")) return "mikrotik";
        if (Has(words, "web") || Has(words, "html") || Has(words, "sitio") || Has(words, "pagina") || Has(words, "api") || Has(words, "http") || Has(words, "rest")) return "web";
        if (Has(words, "codigo") || Has(words, "programar") || Has(words, "script") || Has(words, "funcion") || Has(words, "programa") || Has(words, "clase") || Has(words, "metodo")) return "programacion";
        if (Has(words, "obra") || Has(words, "construccion") || Has(words, "hormigon") || Has(words, "cemento") || Has(words, "losa") || Has(words, "hierro") || Has(words, "muro") || Has(words, "cimiento")) return "construccion";
        if (Has(words, "iva") || Has(words, "factura") || Has(words, "impuesto") || Has(words, "costo") || Has(words, "precio") || Has(words, "presupuesto") || Has(words, "amortizacion")) return "finanzas";
        if (Has(words, "formula") || Has(words, "calculo") || Has(words, "ecuacion") || Has(words, "modulo") || Has(words, "derivada") || Has(words, "integral") || Has(words, "estadistica")) return "matematicas";
        if (Has(words, "ia") || Has(words, "inteligencia") || Has(words, "aprendizaje") || Has(words, "modelo") || Has(words, "neuronal") || Has(words, "entrenamiento")) return "ia";
        if (Has(words, "seguridad") || Has(words, "vulnerabilidad") || Has(words, "cifrado") || Has(words, "ssl") || Has(words, "tls") || Has(words, "certificado")) return "seguridad";
        if (Has(words, "fisica") || Has(words, "quimica") || Has(words, "biologia") || Has(words, "ciencia") || Has(words, "energia") || Has(words, "fuerza")) return "ciencia";
        return "general";
    }

    // -------------------------------------------------------------------------
    //  MEMORY ENGINE
    // -------------------------------------------------------------------------

    static MemoryMatch BestMemory(string prompt, float minCoverage)
    {
        var best = new MemoryMatch("", 0);
        if (!File.Exists(MemoryFile)) return best;
        string[] promptWords = Words(prompt);
        if (promptWords.Length == 0) return best;

        foreach (string line in File.ReadAllLines(MemoryFile, Encoding.UTF8))
        {
            string[] parts = line.Split('\t');
            if (parts.Length < 2) continue;
            string key = Unescape(parts[0]);
            int score = Score(promptWords, Words(key));
            if (score >= 1 && (float)score / promptWords.Length >= minCoverage && score > best.Score)
                best = new MemoryMatch(Unescape(parts[1]), score);
        }
        return best;
    }

    // Word overlap score: count how many prompt words appear in memory key
    static int Score(string[] prompt, string[] memory)
    {
        int s = 0;
        foreach (string x in prompt)
        {
            if (x.Length < 3) continue;
            foreach (string y in memory)
            {
                if (x == y) { s++; break; }
            }
        }
        return s;
    }

    // -------------------------------------------------------------------------
    //  STRING UTILITIES
    // -------------------------------------------------------------------------

    static string[] Words(string text)
    {
        return Normalize(text).Split(
            new char[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '/', '\\', '-', '_', '?', '!' },
            StringSplitOptions.RemoveEmptyEntries
        );
    }

    static bool Has(string[] words, string target)
    {
        string t = Normalize(target);
        foreach (string w in words)
            if (w == t) return true;
        return false;
    }

    // FIXED: proper Unicode NFD normalization strips tildes and accents correctly.
    // Old version used literal "????" sequences which never matched real Spanish text.
    static string Normalize(string t)
    {
        if (t == null) return "";
        t = t.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(t.Length);
        foreach (char c in t)
        {
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }
        return result.ToString();
    }

    static string EscapeForIan(string t) { return (t ?? "").Replace("\\", "\\\\").Replace("\"", "\\\""); }

    static string Escape(string t)
    {
        return (t ?? "")
            .Replace("\\", "\\\\")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    static string Unescape(string t)
    {
        return (t ?? "")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\\", "\\");
    }

    static string[] Slice(string[] input, int start)
    {
        var output = new string[input.Length - start];
        for (int j = start; j < input.Length; j++)
            output[j - start] = input[j];
        return output;
    }
}

struct MemoryMatch
{
    public string Program;
    public int Score;
    public MemoryMatch(string p, int s) { Program = p; Score = s; }
}

