using System;
using System.Collections.Generic;
using System.Diagnostics;        
using System.Globalization;      
using System.IO;
using System.Net;
using System.Net.Sockets;        
using System.Text;

class IanEngine
{
    const string Version = "0.5.0";

    readonly Dictionary<string, object> vars = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, Bridge> bridges = new Dictionary<string, Bridge>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, IanFunction> functions = new Dictionary<string, IanFunction>(StringComparer.OrdinalIgnoreCase);
    readonly string baseDir;
    readonly bool allowRun;
    int lineaActual = 0;

    IanEngine(string baseDir, bool allowRun)
    {
        this.baseDir = Path.GetFullPath(baseDir);
        this.allowRun = allowRun;
    }

    public static int Run(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding  = System.Text.Encoding.UTF8;
        bool allowRun = false;
        bool showVersion = false;
        string file = null;

        foreach (string arg in args)
        {
            if (arg == "--allow-run") allowRun = true;
            else if (arg == "--version") showVersion = true;
            else if (file == null) file = arg;
            else return Fail("Argumento desconocido: " + arg);
        }

        if (showVersion)
        {
            Console.WriteLine("i@N " + Version);
            return 0;
        }

        if (file == null)
        {
            Console.WriteLine("Uso: ian.exe [--allow-run] archivo.ian");
            return 0;
        }

        try
        {
            string full = Path.GetFullPath(file);
            string source = File.ReadAllText(full, Encoding.UTF8);
            new IanEngine(Path.GetDirectoryName(full), allowRun).Run(source);
            return 0;
        }
        catch (IanException ex)
        {
            Console.Error.WriteLine("i@N error: " + ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N error inesperado: " + ex.Message);
            return 1;
        }
    }

    static int Fail(string message)
    {
        Console.Error.WriteLine("i@N error: " + message);
        return 1;
    }

    void Run(string source)
    {
        string[] lines = source.Replace("\r\n", "\n").Split('\n');
        try
        {
            RunLines(lines, 0, lines.Length);
        }
        catch (IanException ex)
        {
            throw new IanException("linea " + lineaActual + ": " + ex.Message);
        }
    }

    void RunLines(string[] lines, int start, int end)
    {
        int i = start;
        while (i < end)
        {
            lineaActual = i + 1;
            string line = Clean(lines[i]);
            i++;
            if (line.Length == 0) continue;

            if (line.StartsWith("funcion ", StringComparison.OrdinalIgnoreCase))
            {
                int blockStart = i;
                int depth = 0;
                while (i < end)
                {
                    string cur = Clean(lines[i]);
                    if (IsBlockStart(cur)) depth++;
                    if (cur.Equals("fin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (depth == 0) break;
                        depth--;
                    }
                    i++;
                }
                if (i >= end) throw new IanException("Bloque 'funcion' sin 'fin' (linea " + lineaActual + ")"); 
                CmdDefinirFuncion(line, lines, blockStart, i);
                i++;
                continue;
            }

            if (line.StartsWith("repetir ", StringComparison.OrdinalIgnoreCase))
            {
                int blockStart = i;
                int depth = 0;
                while (i < end)
                {
                    string cur = Clean(lines[i]);
                    if (IsBlockStart(cur)) depth++;
                    if (cur.Equals("fin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (depth == 0) break;
                        depth--;
                    }
                    i++;
                }
                if (i >= end) throw new IanException("Bloque 'repetir' sin 'fin' (linea " + lineaActual + ")"); 
                
                List<string> rParts = Split(line);
                if (rParts.Count >= 6 && rParts[1].ToLowerInvariant() == "para" && rParts[2].ToLowerInvariant() == "cada")
                {
                    string varName = rParts[3];
                    List<object> list = new List<object>(GetLista(rParts[5]));
                    foreach (object item in list)
                    {
                        vars[varName] = item;
                        RunLines(lines, blockStart, i);
                    }
                }
                else
                {
                    int count = ParseRepeat(line);
                    for (int n = 0; n < count; n++) RunLines(lines, blockStart, i);
                }
                i++;
                continue;
            }

            if (line.StartsWith("si ", StringComparison.OrdinalIgnoreCase) && line.EndsWith(" entonces", StringComparison.OrdinalIgnoreCase))
            {
                int blockStart = i;
                int sinoAt = -1;
                int depth = 0;
                while (i < end)
                {
                    string cur = Clean(lines[i]);
                    if (IsBlockStart(cur)) depth++;
                    if (cur.Equals("sino", StringComparison.OrdinalIgnoreCase) && depth == 0) sinoAt = i;       
                    if (cur.Equals("fin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (depth == 0) break;
                        depth--;
                    }
                    i++;
                }
                if (i >= end) throw new IanException("Bloque 'si' sin 'fin' (linea " + lineaActual + ")");      
                int finAt = i;
                bool cond = EvalCondicion(line);
                if (cond) RunLines(lines, blockStart, sinoAt >= 0 ? sinoAt : finAt);
                else if (sinoAt >= 0) RunLines(lines, sinoAt + 1, finAt);
                i = finAt + 1;
                continue;
            }

            if (line.StartsWith("mientras ", StringComparison.OrdinalIgnoreCase))
            {
                int blockStart = i;
                int depth = 0;
                while (i < end)
                {
                    string cur = Clean(lines[i]);
                    if (IsBlockStart(cur)) depth++;
                    if (cur.Equals("fin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (depth == 0) break;
                        depth--;
                    }
                    i++;
                }
                if (i >= end) throw new IanException("Bloque 'mientras' sin 'fin' (linea " + lineaActual + ")");
                int finAt = i;
                string condMientras = line.Substring(8).Trim();
                int maxIter = 100000;
                while (EvalCondStr(condMientras) && maxIter-- > 0)
                    RunLines(lines, blockStart, finAt);
                if (maxIter <= 0) throw new IanException("Bucle 'mientras' excedio 100000 iteraciones (posible bucle infinito)");
                i = finAt + 1;
                continue;
            }

            Execute(line);
        }
    }

    bool IsBlockStart(string line)
    {
        return (line.StartsWith("si ", StringComparison.OrdinalIgnoreCase) && line.EndsWith(" entonces", StringComparison.OrdinalIgnoreCase))
            || line.StartsWith("mientras ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("repetir ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("funcion ", StringComparison.OrdinalIgnoreCase);
    }

    void Execute(string line)
    {
        List<string> parts = Split(line);
        if (parts.Count == 0) return;
        string head = parts[0].ToLowerInvariant();

        if (head == "decir")
        {
            var sb = new StringBuilder();
            for (int j = 1; j < parts.Count; j++)
            {
                if (j > 1) sb.Append(" ");
                sb.Append(Format(Value(parts[j])));
            }
            Console.WriteLine(sb.ToString());
            return;
        }
        if (head == "ia")          { CmdIa(parts); return; }
        if (head == "depurar")     { CmdDepurar(); return; }
        if (head == "incluir")     { CmdIncluir(parts); return; }
        if (head == "archivo")     { CmdArchivo(parts); return; }
        if (head == "guardar")     { CmdGuardar(parts); return; }
        if (head == "sumar")       { CmdMatematica(parts, "sumar"); return; }
        if (head == "restar")      { CmdMatematica(parts, "restar"); return; }
        if (head == "multiplicar") { CmdMatematica(parts, "multiplicar"); return; }
        if (head == "dividir")     { CmdMatematica(parts, "dividir"); return; }
        if (head == "modulo")      { CmdMatematica(parts, "modulo"); return; }
        if (head == "incrementar") { CmdIncrementar(parts, 1); return; }
        if (head == "decrementar") { CmdIncrementar(parts, -1); return; }
        if (head == "texto")       { CmdTexto(parts); return; }
        if (head == "unir")        { CmdUnir(parts); return; }
        if (head == "lista")       { CmdLista(parts); return; }
        if (head == "api")         { CmdApi(parts); return; }
        if (head == "red")         { CmdRed(parts); return; }
        if (head == "web")         { CmdWeb(parts); return; }
        if (head == "mikrotik")    { CmdMikrotik(parts); return; }
        if (head == "agente")      { CmdAgente(parts); return; }
        if (head == "nexo")        { CmdNexo(parts); return; }
        if (head == "llamar")      { CmdLlamar(parts); return; }
        if (head == "excel")       { CmdExcel(parts); return; }
        if (head == "word")        { CmdWord(parts); return; }

        throw new IanException("Comando desconocido: " + head);
    }

    // --- Comandos ------------------------------------------------------------

    void CmdIa(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: ia decir|texto|pensar|limpiar|preguntar|consultar|reflexionar");
        string action = parts[1].ToLowerInvariant();

        if (action == "decir")
        {
            Console.Write("i@N >> ");
            Console.WriteLine(Format(Value(Join(parts, 2, parts.Count))));
            return;
        }

        if (action == "texto")
        {
            Console.Write(Format(Value(Join(parts, 2, parts.Count))));
            return;
        }

        if (action == "pensar")
        {
            Console.Write("i@N esta pensando... ");
            System.Threading.Thread.Sleep(600);
            Console.WriteLine("Listo.");
            return;
        }

        if (action == "limpiar")
        {
            try { Console.Clear(); } catch { }
            return;
        }

        if (action == "preguntar")
        {
            if (parts.Count < 5 || parts[parts.Count - 2].ToLowerInvariant() != "como")
                throw new IanException("Uso: ia preguntar \"pregunta\" como variable");
            Console.Write("i@N >> " + Format(Value(Join(parts, 2, parts.Count - 2))) + " ");
            vars[parts[parts.Count - 1]] = Console.ReadLine();
            return;
        }

        if (action == "consultar")
        {
            // ia consultar "pregunta" como variable -> Llama a ian.exe brain preguntar
            if (parts.Count < 5 || parts[parts.Count - 2].ToLowerInvariant() != "como")
                throw new IanException("Uso: ia consultar \"pregunta\" como variable");
            string question = Convert.ToString(Value(Join(parts, 2, parts.Count - 2)));
            string output;
            RunProcessCapture(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ian.exe"), "brain preguntar " + QuoteArg(question), "", baseDir, out output);
            vars[parts[parts.Count - 1]] = output.Trim();
            return;
        }

        if (action == "reflexionar")
        {
            // ia reflexionar "pedido" como variable -> Llama a ian.exe agent (sin --run) para obtener el programa i@N
            if (parts.Count < 5 || parts[parts.Count - 2].ToLowerInvariant() != "como")
                throw new IanException("Uso: ia reflexionar \"pedido\" como variable");
            string pedido = Convert.ToString(Value(Join(parts, 2, parts.Count - 2)));
            string output;
            RunProcessCapture(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ian.exe"), "agent " + QuoteArg(pedido), "", baseDir, out output);
            vars[parts[parts.Count - 1]] = output.Trim();
            return;
        }

        if (action == "guardar")
        {
            // ia guardar "nombre.txt" "contenido\ncon saltos de linea"
            // Siempre guarda en el Escritorio. Nunca usar rutas absolutas.
            if (parts.Count < 3) throw new IanException("Uso: ia guardar \"nombre.txt\" \"contenido\"");

            // Extraer nombre SIN llamar a Value() ? evita que \t en rutas Windows se convierta en TAB
            string rawToken = parts[2];
            string rawPath;
            if (rawToken.Length >= 2 && rawToken[0] == '"' && rawToken[rawToken.Length - 1] == '"')
                rawPath = rawToken.Substring(1, rawToken.Length - 2);
            else if (rawToken.Length >= 2 && rawToken[0] == '\'' && rawToken[rawToken.Length - 1] == '\'')
                rawPath = rawToken.Substring(1, rawToken.Length - 2);
            else
                rawPath = rawToken;

            // Tomar solo el nombre de archivo (sin ruta absoluta)
            int lastSep = Math.Max(rawPath.LastIndexOf('\\'), rawPath.LastIndexOf('/'));
            string fileName = lastSep >= 0 ? rawPath.Substring(lastSep + 1) : rawPath;
            foreach (char inv in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(inv, '_');
            if (string.IsNullOrEmpty(fileName)) fileName = "ian_archivo.txt";

            string target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            string content = parts.Count >= 4 ? Format(Value(Join(parts, 3, parts.Count))) : "";
            File.WriteAllText(target, content, Encoding.UTF8);
            Console.WriteLine("i@N >> Guardado: " + target);
            return;
        }

        throw new IanException("Accion IA desconocida: " + action);
    }

    void CmdDepurar()
    {
        Console.WriteLine("--- ESTADO DE VARIABLES ---");
        foreach (var kvp in vars)
        {
            Console.WriteLine("  $" + kvp.Key + " = " + Format(kvp.Value));
        }
        Console.WriteLine("---------------------------");
    }

    void CmdGuardar(List<string> parts)
    {
        if (parts.Count < 4 || parts[2] != "=") throw new IanException("Uso: guardar nombre = valor");
        vars[parts[1]] = Value(Join(parts, 3, parts.Count));
    }

    void CmdIncluir(List<string> parts)
    {
        if (parts.Count != 2) throw new IanException("Uso: incluir \"archivo.ian\"");
        string target = SafePath(Convert.ToString(Value(parts[1])));
        if (!File.Exists(target)) throw new IanException("No existe el archivo incluido: " + target);
        Run(File.ReadAllText(target, Encoding.UTF8));
    }

    void CmdArchivo(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: archivo leer|escribir|agregar|existe");
        string action = parts[1].ToLowerInvariant();

        if (action == "leer")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: archivo leer \"ruta\" como variable");
            string target = SafePath(Convert.ToString(Value(parts[2])));
            if (!File.Exists(target)) throw new IanException("No existe el archivo: " + target);
            vars[parts[4]] = File.ReadAllText(target, Encoding.UTF8);
            return;
        }

        if (action == "escribir")
        {
            if (parts.Count < 4) throw new IanException("Uso: archivo escribir \"ruta\" contenido");
            string target = SafePath(Convert.ToString(Value(parts[2])));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.WriteAllText(target, Format(Value(Join(parts, 3, parts.Count))), Encoding.UTF8);
            return;
        }

        if (action == "agregar")
        {
            if (parts.Count < 4) throw new IanException("Uso: archivo agregar \"ruta\" contenido");
            string target = SafePath(Convert.ToString(Value(parts[2])));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.AppendAllText(target, Format(Value(Join(parts, 3, parts.Count))), Encoding.UTF8);
            return;
        }

        if (action == "existe")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: archivo existe \"ruta\" como variable");
            vars[parts[4]] = File.Exists(SafePath(Convert.ToString(Value(parts[2]))));
            return;
        }

        throw new IanException("Accion de archivo desconocida: " + action);
    }

    // --- Matematica ----------------------------------------------------------

    void CmdMatematica(List<string> parts, string op)
    {
        if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: " + op + " a b como resultado");
        double a = Convert.ToDouble(Value(parts[1]), CultureInfo.InvariantCulture);
        double b = Convert.ToDouble(Value(parts[2]), CultureInfo.InvariantCulture);
        double res;
        if (op == "sumar")            res = a + b;
        else if (op == "restar")      res = a - b;
        else if (op == "multiplicar") res = a * b;
        else if (op == "dividir")
        {
            if (b == 0) throw new IanException("Division por cero");
            res = a / b;
        }
        else if (op == "modulo")
        {
            if (b == 0) throw new IanException("Modulo por cero");
            res = a % b;
        }
        else throw new IanException("Operacion desconocida: " + op);
        vars[parts[4]] = res;
    }

    void CmdIncrementar(List<string> parts, double delta)
    {
        if (parts.Count != 2) throw new IanException("Uso: incrementar nombre / decrementar nombre");
        string name = parts[1];
        if (!vars.ContainsKey(name)) throw new IanException("Variable no definida: " + name);
        vars[name] = Convert.ToDouble(vars[name], CultureInfo.InvariantCulture) + delta;
    }

    // --- Texto ---------------------------------------------------------------

    void CmdTexto(List<string> parts)
    {
        if (parts.Count < 3) throw new IanException("Uso: texto longitud|mayusculas|minusculas|contiene|recortar ...");
        string action = parts[1].ToLowerInvariant();

        if (action == "longitud")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: texto longitud variable como resultado");
            vars[parts[4]] = (double)Convert.ToString(Value(parts[2]), CultureInfo.InvariantCulture).Length;    
        }
        else if (action == "mayusculas" || action == "minusculas")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: texto " + action + " variable como resultado");
            string s = Convert.ToString(Value(parts[2]), CultureInfo.InvariantCulture);
            vars[parts[4]] = action == "mayusculas" ? s.ToUpperInvariant() : s.ToLowerInvariant();
        }
        else if (action == "contiene")
        {
            if (parts.Count != 6 || parts[4] != "como") throw new IanException("Uso: texto contiene texto buscar como resultado");
            string hay    = Convert.ToString(Value(parts[2]), CultureInfo.InvariantCulture);
            string needle = Convert.ToString(Value(parts[3]), CultureInfo.InvariantCulture);
            vars[parts[5]] = hay.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        else if (action == "recortar")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: texto recortar variable como resultado");
            vars[parts[4]] = Convert.ToString(Value(parts[2]), CultureInfo.InvariantCulture).Trim();
        }
        else throw new IanException("Accion de texto desconocida: " + action);
    }

    void CmdUnir(List<string> parts)
    {
        if (parts.Count < 4 || parts[parts.Count - 2] != "como")
            throw new IanException("Uso: unir valor1 valor2 ... como resultado");
        var sb = new StringBuilder();
        for (int i = 1; i < parts.Count - 2; i++)
            sb.Append(Convert.ToString(Value(parts[i]), CultureInfo.InvariantCulture));
        vars[parts[parts.Count - 1]] = sb.ToString();
    }

    // --- Lista ---------------------------------------------------------------

    void CmdLista(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: lista crear|agregar|obtener|tamanno|json");
        string action = parts[1].ToLowerInvariant();

        if (action == "crear")
        {
            if (parts.Count != 4 || parts[2] != "como") throw new IanException("Uso: lista crear como nombre"); 
            vars[parts[3]] = new List<object>();
        }
        else if (action == "agregar")
        {
            if (parts.Count < 4) throw new IanException("Uso: lista agregar nombre valor");
            GetLista(parts[2]).Add(Value(Join(parts, 3, parts.Count)));
        }
        else if (action == "obtener")
        {
            if (parts.Count != 6 || parts[4] != "como") throw new IanException("Uso: lista obtener nombre indice como resultado");
            List<object> lst = GetLista(parts[2]);
            int idx = (int)Convert.ToDouble(Value(parts[3]), CultureInfo.InvariantCulture);
            if (idx < 0 || idx >= lst.Count) throw new IanException("Lista: indice " + idx + " fuera de rango (0-" + (lst.Count - 1) + ")");
            vars[parts[5]] = lst[idx];
        }
        else if (action == "tamanno" || action == "tamanio" || action == "tamano" || action == "tama??o")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: lista tamanno nombre como resultado");
            vars[parts[4]] = (double)GetLista(parts[2]).Count;
        }
        else if (action == "json")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: lista json nombre como resultado");
            List<object> lst = GetLista(parts[2]);
            var sb2 = new StringBuilder("[");
            for (int k = 0; k < lst.Count; k++) { if (k > 0) sb2.Append(", "); sb2.Append(ToJson(lst[k])); }    
            sb2.Append("]");
            vars[parts[4]] = sb2.ToString();
        }
        else throw new IanException("Accion de lista desconocida: " + action);
    }

    List<object> GetLista(string name)
    {
        if (!vars.ContainsKey(name) || !(vars[name] is List<object>))
            throw new IanException("No es una lista: " + name);
        return (List<object>)vars[name];
    }

    // --- API -----------------------------------------------------------------
    // Sintaxis:
    //   api get "url" como resultado
    //   api get "url" bearer "TOKEN" como resultado
    //   api get "url" cabecera "Header" "Valor" como resultado
    //   api post "url" "body-json" como resultado
    //   api post "url" "body-json" bearer "TOKEN" como resultado
    //   api put "url" "body-json" como resultado
    //   api delete "url" como resultado

    void CmdApi(List<string> parts)
    {
        if (parts.Count < 4 || parts[parts.Count - 2].ToLowerInvariant() != "como")
            throw new IanException("Uso: api get|post|put|delete \"url\" [\"body\"] [bearer \"token\"] como resultado");

        string method = parts[1].ToLowerInvariant();
        if (method != "get" && method != "post" && method != "put" && method != "delete" && method != "patch")
            throw new IanException("Metodo no soportado. Usa: get, post, put, delete, patch");

        string outputVar = parts[parts.Count - 1];
        string url = Convert.ToString(Value(parts[2]));

        // Parse optional body, bearer and custom header from remaining tokens
        string body         = "";
        string bearerToken  = "";
        string headerName   = "";
        string headerValue  = "";

        int pos = 3;
        int limit = parts.Count - 2; // stop before "como resultado"

        // Body: optional for post/put/patch ? next token if not a keyword
        if (pos < limit)
        {
            string peek = parts[pos].ToLowerInvariant();
            bool isKeyword = peek == "bearer" || peek == "cabecera" || peek == "como";
            if (!isKeyword && (method == "post" || method == "put" || method == "patch"))
            {
                body = Convert.ToString(Value(parts[pos]));
                pos++;
            }
        }

        // bearer "TOKEN"
        if (pos < limit && parts[pos].ToLowerInvariant() == "bearer" && pos + 1 < limit)
        {
            bearerToken = Convert.ToString(Value(parts[pos + 1]));
            pos += 2;
        }

        // cabecera "Nombre" "Valor"
        if (pos < limit && parts[pos].ToLowerInvariant() == "cabecera" && pos + 2 < limit)
        {
            headerName  = Convert.ToString(Value(parts[pos + 1]));
            headerValue = Convert.ToString(Value(parts[pos + 2]));
            pos += 3;
        }

        try
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method     = method.ToUpperInvariant();
            req.Timeout    = 20000;
            req.UserAgent  = "i@N/" + Version;

            if (!string.IsNullOrEmpty(bearerToken))
                req.Headers.Add("Authorization", "Bearer " + bearerToken);
            if (!string.IsNullOrEmpty(headerName))
                req.Headers.Add(headerName, headerValue);

            if (!string.IsNullOrEmpty(body) && method != "get" && method != "delete")
            {
                req.ContentType = "application/json";
                byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                req.ContentLength = bodyBytes.Length;
                using (var s = req.GetRequestStream()) s.Write(bodyBytes, 0, bodyBytes.Length);
            }

            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var sr   = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                vars[outputVar] = sr.ReadToEnd().Trim();
        }
        catch (WebException ex)
        {
            string errBody = "";
            try
            {
                if (ex.Response != null)
                    using (var sr = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                        errBody = sr.ReadToEnd().Trim();
            }
            catch { }
            vars[outputVar] = "{ok: falso, error: \"" + Escape(ex.Message) + "\", cuerpo: \"" + Escape(errBody) + "\"}";
        }
        catch (Exception ex)
        {
            vars[outputVar] = "{ok: falso, error: \"" + Escape(ex.Message) + "\"}";
        }
    }

    // --- Red -----------------------------------------------------------------

    void CmdRed(List<string> parts)
    {
        if (parts.Count < 5 || parts[parts.Count - 2] != "como") throw new IanException("Uso: red dns dominio como variable | red puerto host puerto como variable");
        string action = parts[1].ToLowerInvariant();
        string output = parts[parts.Count - 1];

        if (action == "dns")
        {
            string host = Convert.ToString(Value(parts[2]));
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                vars[output] = "{ok: verdadero, ips: [" + QuoteList(addresses) + "]}";
            }
            catch (Exception ex)
            {
                vars[output] = "{ok: falso, error: \"" + Escape(ex.Message) + "\"}";
            }
            return;
        }

        if (action == "puerto")
        {
            string host = Convert.ToString(Value(parts[2]));
            int port = Convert.ToInt32(Value(parts[3]), CultureInfo.InvariantCulture);
            vars[output] = TcpOpen(host, port);
            return;
        }

        if (action == "ping")
        {
            if (!allowRun) throw new IanException("red ping requiere --allow-run");
            string host = Convert.ToString(Value(parts[2]));
            vars[output] = RunProcess("ping", (Environment.OSVersion.Platform == PlatformID.Win32NT ? "-n 1 " : "-c 1 ") + QuoteArg(host), "") == 0;
            return;
        }

        throw new IanException("Accion de red desconocida: " + action);
    }

    // --- Web -----------------------------------------------------------------

    void CmdWeb(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: web iniciar|titulo|subtitulo|texto|boton|tabla|fila|tabla-fin|lista|imagen|enlace|css|seccion|seccion-fin|formulario|campo|formulario-fin|guardar|pdf");
        string action = parts[1].ToLowerInvariant();

        if (action == "iniciar")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: web iniciar \"Titulo\" como pagina");
            vars[parts[4]] = new Page(Convert.ToString(Value(parts[2])));
            return;
        }

        if (action == "titulo")
        {
            if (parts.Count < 4) throw new IanException("Uso: web titulo pagina \"texto\"");
            Page page = GetPage(parts[2]);
            page.Parts.Add("<h1 class=\"ian-h1\">" + Html(Convert.ToString(Value(Join(parts, 3, parts.Count)))) + "</h1>");
            return;
        }

        if (action == "subtitulo")
        {
            if (parts.Count < 4) throw new IanException("Uso: web subtitulo pagina \"texto\"");
            Page page = GetPage(parts[2]);
            page.Parts.Add("<h2 class=\"ian-h2\">" + Html(Convert.ToString(Value(Join(parts, 3, parts.Count)))) + "</h2>");
            return;
        }

        if (action == "texto")
        {
            if (parts.Count < 4) throw new IanException("Uso: web texto pagina \"texto\"");
            Page page = GetPage(parts[2]);
            page.Parts.Add("<p class=\"ian-p\">" + Html(Convert.ToString(Value(Join(parts, 3, parts.Count)))) + "</p>");
            return;
        }

        if (action == "boton")
        {
            if (parts.Count < 6 || parts[4] != "mensaje") throw new IanException("Uso: web boton pagina \"Etiqueta\" mensaje \"Texto\"");
            Page page = GetPage(parts[2]);
            string label = Html(Convert.ToString(Value(parts[3])));
            string message = Escape(Convert.ToString(Value(Join(parts, 5, parts.Count))));
            page.Parts.Add("<button class=\"ian-btn\" onclick=\"alert('" + message + "')\">" + label + "</button>");
            return;
        }

        if (action == "lista")
        {
            // web lista pagina "item1,item2,item3"
            if (parts.Count < 4) throw new IanException("Uso: web lista pagina \"item1,item2,item3\"");
            Page page = GetPage(parts[2]);
            string[] items = Convert.ToString(Value(parts[3])).Split(',');
            var sb = new StringBuilder("<ul class=\"ian-ul\">");
            foreach (string item in items)
                sb.Append("<li>").Append(Html(item.Trim())).Append("</li>");
            sb.Append("</ul>");
            page.Parts.Add(sb.ToString());
            return;
        }

        if (action == "imagen")
        {
            // web imagen pagina "url" "alt"
            if (parts.Count < 4) throw new IanException("Uso: web imagen pagina \"url\" \"descripcion\"");
            Page page = GetPage(parts[2]);
            string url = Html(Convert.ToString(Value(parts[3])));
            string alt = parts.Count >= 5 ? Html(Convert.ToString(Value(parts[4]))) : "";
            page.Parts.Add("<img class=\"ian-img\" src=\"" + url + "\" alt=\"" + alt + "\">");
            return;
        }

        if (action == "enlace")
        {
            // web enlace pagina "texto" "url"
            if (parts.Count < 5) throw new IanException("Uso: web enlace pagina \"texto\" \"url\"");
            Page page = GetPage(parts[2]);
            string text = Html(Convert.ToString(Value(parts[3])));
            string url = Html(Convert.ToString(Value(parts[4])));
            page.Parts.Add("<p><a class=\"ian-link\" href=\"" + url + "\" target=\"_blank\">" + text + "</a></p>");
            return;
        }

        if (action == "css")
        {
            // web css pagina "selector { propiedad: valor; }"
            if (parts.Count < 4) throw new IanException("Uso: web css pagina \"regla css\"");
            Page page = GetPage(parts[2]);
            page.CustomCss.Add(Convert.ToString(Value(Join(parts, 3, parts.Count))));
            return;
        }

        if (action == "seccion")
        {
            // web seccion pagina "id-clase"
            if (parts.Count < 4) throw new IanException("Uso: web seccion pagina \"nombre\"");
            Page page = GetPage(parts[2]);
            string id = Html(Convert.ToString(Value(parts[3])));
            page.Parts.Add("<div class=\"ian-seccion\" id=\"" + id + "\">");
            return;
        }

        if (action == "seccion-fin")
        {
            Page page = GetPage(parts[2]);
            page.Parts.Add("</div>");
            return;
        }

        if (action == "tabla")
        {
            // web tabla pagina "Col1,Col2,Col3"
            if (parts.Count < 4) throw new IanException("Uso: web tabla pagina \"Col1,Col2,Col3\"");
            Page page = GetPage(parts[2]);
            string[] cols = Convert.ToString(Value(parts[3])).Split(',');
            page.TableBuf = new StringBuilder();
            page.TableBuf.Append("<table class=\"ian-table\"><thead><tr>");
            foreach (string col in cols)
                page.TableBuf.Append("<th>").Append(Html(col.Trim())).Append("</th>");
            page.TableBuf.Append("</tr></thead><tbody>");
            return;
        }

        if (action == "fila")
        {
            // web fila pagina "Val1,Val2,Val3"
            if (parts.Count < 4) throw new IanException("Uso: web fila pagina \"Val1,Val2,Val3\"");
            Page page = GetPage(parts[2]);
            string[] vals = Convert.ToString(Value(parts[3])).Split(',');
            if (page.TableBuf == null) throw new IanException("Usa 'web tabla' antes de 'web fila'");
            page.TableBuf.Append("<tr>");
            foreach (string val in vals)
                page.TableBuf.Append("<td>").Append(Html(val.Trim())).Append("</td>");
            page.TableBuf.Append("</tr>");
            return;
        }

        if (action == "tabla-fin")
        {
            Page page = GetPage(parts[2]);
            if (page.TableBuf == null) throw new IanException("No hay tabla abierta");
            page.TableBuf.Append("</tbody></table>");
            page.Parts.Add(page.TableBuf.ToString());
            page.TableBuf = null;
            return;
        }

        if (action == "formulario")
        {
            // web formulario pagina "accion"
            if (parts.Count < 4) throw new IanException("Uso: web formulario pagina \"url-accion\"");
            Page page = GetPage(parts[2]);
            string formAction = Html(Convert.ToString(Value(parts[3])));
            page.Parts.Add("<form class=\"ian-form\" action=\"" + formAction + "\" method=\"post\">");
            return;
        }

        if (action == "campo")
        {
            // web campo pagina "nombre" "tipo" "placeholder"
            if (parts.Count < 5) throw new IanException("Uso: web campo pagina \"nombre\" \"tipo\" \"placeholder\"");
            Page page = GetPage(parts[2]);
            string name = Html(Convert.ToString(Value(parts[3])));
            string type = Html(Convert.ToString(Value(parts[4])));
            string placeholder = parts.Count >= 6 ? Html(Convert.ToString(Value(parts[5]))) : name;
            page.Parts.Add("<div class=\"ian-campo\"><label>" + placeholder + "</label><input type=\"" + type + "\" name=\"" + name + "\" placeholder=\"" + placeholder + "\"></div>");
            return;
        }

        if (action == "formulario-fin")
        {
            Page page = GetPage(parts[2]);
            page.Parts.Add("<button class=\"ian-btn\" type=\"submit\">Enviar</button></form>");
            return;
        }

        if (action == "guardar")
        {
            if (parts.Count != 5 || parts[3] != "en") throw new IanException("Uso: web guardar pagina en \"archivo.html|pdf\"");
            Page page = GetPage(parts[2]);
            string target = SafePath(Convert.ToString(Value(parts[4])));
            string ext = Path.GetExtension(target).ToLowerInvariant();
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? ".");

            if (ext == ".pdf")
            {
                // Auto-convert HTML ? PDF via browser headless
                string tmpHtml = target + ".tmp.html";
                File.WriteAllText(tmpHtml, RenderPage(page), Encoding.UTF8);
                bool ok = ConvertToPdf(tmpHtml, target);
                try { File.Delete(tmpHtml); } catch { }
                if (ok)
                    Console.WriteLine("pdf: " + target);
                else
                {
                    // Fallback: save HTML instead
                    string fallback = Path.ChangeExtension(target, ".html");
                    File.WriteAllText(fallback, RenderPage(page), Encoding.UTF8);
                    Console.WriteLine("pdf: Edge/Chrome no encontrado. Guardado como HTML: " + fallback);
                }
            }
            else
            {
                File.WriteAllText(target, RenderPage(page), Encoding.UTF8);
                Console.WriteLine("web: " + target);
            }
            return;
        }

        if (action == "pdf")
        {
            // web pdf pagina en "archivo.pdf"
            if (parts.Count != 5 || parts[3] != "en") throw new IanException("Uso: web pdf pagina en \"archivo.pdf\"");
            Page page = GetPage(parts[2]);
            string target = SafePath(Convert.ToString(Value(parts[4])));
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? ".");
            string tmpHtml = target + ".tmp.html";
            File.WriteAllText(tmpHtml, RenderPage(page), Encoding.UTF8);
            bool ok = ConvertToPdf(tmpHtml, target);
            try { File.Delete(tmpHtml); } catch { }
            Console.WriteLine(ok ? "pdf: " + target : "pdf: no se pudo convertir ? guardado en " + tmpHtml);
            return;
        }

        throw new IanException("Accion web desconocida: " + action);
    }

    static bool ConvertToPdf(string htmlPath, string pdfPath)
    {
        string[] browsers = new[] {
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
        };
        foreach (string browser in browsers)
        {
            if (!File.Exists(browser)) continue;
            try
            {
                string fileUrl = "file:///" + htmlPath.Replace('\\', '/');
                string args = "--headless --disable-gpu --no-pdf-header-footer " +
                              "--print-to-pdf=" + QuoteArg(pdfPath) + " " + QuoteArg(fileUrl);
                var psi = new ProcessStartInfo(browser, args);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                using (var p = Process.Start(psi))
                {
                    bool done = p.WaitForExit(30000);
                    if (done && File.Exists(pdfPath) && new FileInfo(pdfPath).Length > 100)
                        return true;
                }
            }
            catch { }
        }
        return false;
    }

    // --- Excel ---------------------------------------------------------------

    void CmdExcel(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: excel crear|encabezado|fila|guardar");
        string action = parts[1].ToLowerInvariant();

        if (action == "crear")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: excel crear \"Hoja\" como variable");
            vars[parts[4]] = new Spreadsheet(Convert.ToString(Value(parts[2])));
            return;
        }
        if (action == "encabezado")
        {
            if (parts.Count < 4) throw new IanException("Uso: excel encabezado variable \"Col1,Col2,...\"");
            GetSpreadsheet(parts[2]).SetHeader(Convert.ToString(Value(parts[3])).Split(','));
            return;
        }
        if (action == "fila")
        {
            if (parts.Count < 4) throw new IanException("Uso: excel fila variable \"Val1,Val2,...\"");
            GetSpreadsheet(parts[2]).AddRow(Convert.ToString(Value(parts[3])).Split(','));
            return;
        }
        if (action == "guardar")
        {
            if (parts.Count != 5 || parts[3] != "en") throw new IanException("Uso: excel guardar variable en \"datos.xls\"");
            Spreadsheet sheet = GetSpreadsheet(parts[2]);
            string target = SafePath(Convert.ToString(Value(parts[4])));
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? ".");
            File.WriteAllText(target, sheet.ToSpreadsheetML(), Encoding.UTF8);
            Console.WriteLine("excel: " + target);
            return;
        }
        throw new IanException("Accion excel desconocida: " + action);
    }

    Spreadsheet GetSpreadsheet(string name)
    {
        if (!vars.ContainsKey(name) || !(vars[name] is Spreadsheet)) throw new IanException("No es una planilla: " + name);
        return (Spreadsheet)vars[name];
    }

    // --- Word ----------------------------------------------------------------

    void CmdWord(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: word crear|titulo|subtitulo|texto|tabla|fila|tabla-fin|guardar");
        string action = parts[1].ToLowerInvariant();

        if (action == "crear")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: word crear \"Titulo\" como variable");
            vars[parts[4]] = new WordDoc(Convert.ToString(Value(parts[2])));
            return;
        }
        if (action == "titulo")
        {
            if (parts.Count < 4) throw new IanException("Uso: word titulo variable \"texto\"");
            GetWordDoc(parts[2]).AddHeading(Convert.ToString(Value(Join(parts, 3, parts.Count))), 1);
            return;
        }
        if (action == "subtitulo")
        {
            if (parts.Count < 4) throw new IanException("Uso: word subtitulo variable \"texto\"");
            GetWordDoc(parts[2]).AddHeading(Convert.ToString(Value(Join(parts, 3, parts.Count))), 2);
            return;
        }
        if (action == "texto")
        {
            if (parts.Count < 4) throw new IanException("Uso: word texto variable \"texto\"");
            GetWordDoc(parts[2]).AddParagraph(Convert.ToString(Value(Join(parts, 3, parts.Count))));
            return;
        }
        if (action == "tabla")
        {
            if (parts.Count < 4) throw new IanException("Uso: word tabla variable \"Col1,Col2,...\"");
            GetWordDoc(parts[2]).StartTable(Convert.ToString(Value(parts[3])).Split(','));
            return;
        }
        if (action == "fila")
        {
            if (parts.Count < 4) throw new IanException("Uso: word fila variable \"Val1,Val2,...\"");
            GetWordDoc(parts[2]).AddTableRow(Convert.ToString(Value(parts[3])).Split(','));
            return;
        }
        if (action == "tabla-fin")
        {
            if (parts.Count < 3) throw new IanException("Uso: word tabla-fin variable");
            GetWordDoc(parts[2]).EndTable();
            return;
        }
        if (action == "guardar")
        {
            if (parts.Count != 5 || parts[3] != "en") throw new IanException("Uso: word guardar variable en \"informe.doc\"");
            WordDoc doc = GetWordDoc(parts[2]);
            string target = SafePath(Convert.ToString(Value(parts[4])));
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? ".");
            File.WriteAllText(target, doc.ToRtf(), Encoding.ASCII);
            Console.WriteLine("word: " + target);
            return;
        }
        throw new IanException("Accion word desconocida: " + action);
    }

    WordDoc GetWordDoc(string name)
    {
        if (!vars.ContainsKey(name) || !(vars[name] is WordDoc)) throw new IanException("No es un documento Word: " + name);
        return (WordDoc)vars[name];
    }

    // --- Mikrotik ------------------------------------------------------------

    void CmdMikrotik(List<string> parts)
    {
        if (parts.Count < 5 || parts[1] != "script" || parts[parts.Count - 2] != "como") throw new IanException("Uso: mikrotik script \"/ip address print\" como variable");
        string command = Convert.ToString(Value(Join(parts, 2, parts.Count - 2)));
        vars[parts[parts.Count - 1]] = "{routeros: \"" + Escape(command) + "\", nota: \"i@N genera el script; la conexion real puede integrarse por nexo.\"}";
    }

    // --- Agente --------------------------------------------------------------

    void CmdAgente(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: agente plan|paso|json");
        string action = parts[1].ToLowerInvariant();

        if (action == "plan")
        {
            if (parts.Count < 5 || parts[parts.Count - 2] != "como") throw new IanException("Uso: agente plan \"objetivo\" como nombre");
            vars[parts[parts.Count - 1]] = new AgentPlan(Convert.ToString(Value(Join(parts, 2, parts.Count - 2))));
            return;
        }

        if (action == "paso")
        {
            if (parts.Count < 5) throw new IanException("Uso: agente paso plan hacer \"accion\"");
            AgentPlan plan = GetPlan(parts[2]);
            string status = parts[3].ToLowerInvariant();
            if (status != "hacer" && status != "haciendo" && status != "hecho") throw new IanException("Estado invalido: hacer, haciendo o hecho");
            plan.Steps.Add(new AgentStep(status, Convert.ToString(Value(Join(parts, 4, parts.Count)))));        
            return;
        }

        if (action == "json")
        {
            if (parts.Count != 5 || parts[3] != "como") throw new IanException("Uso: agente json plan como variable");
            vars[parts[4]] = GetPlan(parts[2]);
            return;
        }

        throw new IanException("Accion agente desconocida: " + action);
    }

    // --- Nexo / Llamar -------------------------------------------------------

    void CmdNexo(List<string> parts)
    {
        if (parts.Count < 5 || parts[2] != "=" || parts[3] != "proceso") throw new IanException("Uso: nexo nombre = proceso \"comando\"");
        bridges[parts[1]] = new Bridge(Convert.ToString(Value(Join(parts, 4, parts.Count))));
    }

    void CmdLlamar(List<string> parts)
    {
        if (parts.Count < 2) throw new IanException("Uso: llamar nombre [args] [como variable]");
        string name = parts[1];

        if (bridges.ContainsKey(name))
        {
            if (parts.Count < 6 || parts[2] != "con" || parts[parts.Count - 2] != "como") throw new IanException("Uso: llamar nexo con \"dato\" como variable");
            if (!allowRun) throw new IanException("llamar nexos externos requiere --allow-run");

            Bridge bridge = bridges[name];
            string payload = ToJson(Value(Join(parts, 3, parts.Count - 2)));
            string output;
            int code = RunProcessCapture(bridge.Program, bridge.Arguments, payload, baseDir, out output);
            if (code != 0) throw new IanException(output.Trim().Length == 0 ? "Nexo fallo con codigo " + code : output.Trim());
            vars[parts[parts.Count - 1]] = output.Trim();
            return;
        }

        if (functions.ContainsKey(name))
        {
            IanFunction func = functions[name];
            int argCount = func.Arguments.Count;
            int providedArgs = parts.Count - 2;
            string outputVar = null;

            if (parts.Count >= 4 && parts[parts.Count - 2].ToLowerInvariant() == "como")
            {
                outputVar = parts[parts.Count - 1];
                providedArgs -= 2;
            }

            if (providedArgs < argCount) 
                throw new IanException("La funcion '" + name + "' requiere " + argCount + " argumentos, se dieron " + providedArgs);

            var backup = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var argName in func.Arguments) if (vars.ContainsKey(argName)) backup[argName] = vars[argName];

            for (int k = 0; k < argCount; k++) vars[func.Arguments[k]] = Value(parts[k + 2]);

            RunLines(func.BodyLines, 0, func.BodyLines.Length);

            if (outputVar != null)
            {
                if (vars.ContainsKey("resultado")) vars[outputVar] = vars["resultado"];
                else vars[outputVar] = "";
            }

            foreach (var argName in func.Arguments)
            {
                if (backup.ContainsKey(argName)) vars[argName] = backup[argName];
                else vars.Remove(argName);
            }
            return;
        }

        throw new IanException("No se encontro nexo o funcion: " + name);
    }

    void CmdDefinirFuncion(string line, string[] lines, int start, int end)
    {
        List<string> parts = Split(line);
        if (parts.Count < 2) throw new IanException("Uso: funcion nombre [arg1 arg2 ...]");
        string name = parts[1];
        List<string> args = new List<string>();
        for (int k = 2; k < parts.Count; k++) args.Add(parts[k]);
        
        string[] body = new string[end - start];
        Array.Copy(lines, start, body, 0, end - start);
        functions[name] = new IanFunction(name, args, body);
    }

    // --- Evaluacion de condiciones -------------------------------------------

    bool EvalCondicion(string lineSi)
    {
        string inner = lineSi.Substring(3);
        inner = inner.Substring(0, inner.Length - 9).Trim();
        return EvalCondStr(inner);
    }

    bool EvalCondStr(string cond)
    {
        List<string> parts = Split(cond);
        if (parts.Count == 0) throw new IanException("Condicion vacia");

        if (parts.Count >= 3 && parts[0].ToLowerInvariant() == "archivo" && parts[1].ToLowerInvariant() == "existe")
        {
            return File.Exists(SafePath(Convert.ToString(Value(parts[2]))));
        }

        if (parts.Count == 1) return IsTruthy(Value(parts[0]));

        for (int i = 0; i < parts.Count; i++)
        {
            string op = parts[i].ToLowerInvariant();

            if (op == "no" && i + 1 < parts.Count && parts[i + 1].ToLowerInvariant() == "es")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 2, parts.Count).Trim();
                return !CompareIgual(Value(l), Value(r));
            }

            if (op == "mayor" && i + 3 < parts.Count && parts[i + 1].ToLowerInvariant() == "o" && parts[i + 2].ToLowerInvariant() == "igual" && parts[i + 3].ToLowerInvariant() == "que")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 4, parts.Count).Trim();
                return CompareNumericos(Value(l), Value(r)) >= 0;
            }

            if (op == "menor" && i + 3 < parts.Count && parts[i + 1].ToLowerInvariant() == "o" && parts[i + 2].ToLowerInvariant() == "igual" && parts[i + 3].ToLowerInvariant() == "que")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 4, parts.Count).Trim();
                return CompareNumericos(Value(l), Value(r)) <= 0;
            }

            if (op == "mayor" && i + 1 < parts.Count && parts[i + 1].ToLowerInvariant() == "que")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 2, parts.Count).Trim();
                return CompareNumericos(Value(l), Value(r)) > 0;
            }

            if (op == "menor" && i + 1 < parts.Count && parts[i + 1].ToLowerInvariant() == "que")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 2, parts.Count).Trim();
                return CompareNumericos(Value(l), Value(r)) < 0;
            }

            if (op == "contiene")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 1, parts.Count).Trim();
                string ls = Convert.ToString(Value(l), CultureInfo.InvariantCulture) ?? "";
                string rs = Convert.ToString(Value(r), CultureInfo.InvariantCulture) ?? "";
                return ls.IndexOf(rs, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (op == "es")
            {
                string l = Join(parts, 0, i).Trim();
                string r = Join(parts, i + 1, parts.Count).Trim();
                return CompareIgual(Value(l), Value(r));
            }
        }

        throw new IanException("Operador no reconocido en condicion: " + cond);
    }

    bool IsTruthy(object val)
    {
        if (val == null) return false;
        if (val is bool) return (bool)val;
        if (val is double) return (double)val != 0;
        string s = Convert.ToString(val, CultureInfo.InvariantCulture);
        return s.Length > 0 && !s.Equals("falso", StringComparison.OrdinalIgnoreCase) && s != "0";
    }

    bool CompareIgual(object a, object b)
    {
        if (a is double || b is double)
        {
            try { return Convert.ToDouble(a, CultureInfo.InvariantCulture) == Convert.ToDouble(b, CultureInfo.InvariantCulture); }
            catch { }
        }
        if (a is bool || b is bool) return IsTruthy(a) == IsTruthy(b);
        return string.Equals(Convert.ToString(a, CultureInfo.InvariantCulture), Convert.ToString(b, CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
    }

    double CompareNumericos(object a, object b)
    {
        return Convert.ToDouble(a, CultureInfo.InvariantCulture) - Convert.ToDouble(b, CultureInfo.InvariantCulture);
    }

    // --- Helpers de valores y variables --------------------------------------

    object Value(string text)
    {
        text = text.Trim();
        if (text.Length == 0) return "";
        if (text.StartsWith("$"))
        {
            string name = text.Substring(1);
            if (!vars.ContainsKey(name)) throw new IanException("Variable no definida: " + name);
            return vars[name];
        }
        if ((text.StartsWith("\"") && text.EndsWith("\"")) || (text.StartsWith("'") && text.EndsWith("'")))     
        {
            return UnescapeString(text.Substring(1, text.Length - 2));
        }
        if (text.Equals("verdadero", StringComparison.OrdinalIgnoreCase)) return true;
        if (text.Equals("falso", StringComparison.OrdinalIgnoreCase)) return false;
        double number;
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out number)) return number;   
        return text;
    }

    static string UnescapeString(string text)
    {
        var builder = new StringBuilder();
        bool escape = false;
        foreach (char c in text)
        {
            if (escape)
            {
                if (c == 'n') builder.Append('\n');
                else if (c == 'r') builder.Append('\r');
                else if (c == 't') builder.Append('\t');
                else builder.Append(c);
                escape = false;
                continue;
            }
            if (c == '\\') { escape = true; continue; }
            builder.Append(c);
        }
        if (escape) builder.Append('\\');
        return builder.ToString();
    }

    Page GetPage(string name)
    {
        if (!vars.ContainsKey(name) || !(vars[name] is Page)) throw new IanException("No es una pagina web: " + name);
        return (Page)vars[name];
    }

    AgentPlan GetPlan(string name)
    {
        if (!vars.ContainsKey(name) || !(vars[name] is AgentPlan)) throw new IanException("No es un plan agente: " + name);
        return (AgentPlan)vars[name];
    }

    int ParseRepeat(string line)
    {
        List<string> parts = Split(line);
        if (parts.Count != 3 || parts[2] != "veces") throw new IanException("Uso: repetir N veces ... fin");    
        int count = (int)Convert.ToDouble(Value(parts[1]), CultureInfo.InvariantCulture);
        if (count < 0) throw new IanException("'repetir' no acepta numeros negativos");
        return count;
    }

    // --- Utilidades estaticas ------------------------------------------------

    static string Clean(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith("#")) return "";
        return trimmed;
    }

    static List<string> Split(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool quote = false;
        char quoteChar = '\0';

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quote)
            {
                // Escaped quote (\") ? stay in quote mode, keep the sequence intact
                if (c == '\\' && i + 1 < line.Length && line[i + 1] == quoteChar)
                {
                    current.Append(c);
                    i++;
                    current.Append(line[i]);
                    continue;
                }
                // Real closing quote ? include it in token, exit quote mode
                if (c == quoteChar)
                {
                    current.Append(c);
                    quote = false;
                    continue;
                }
                current.Append(c);
                continue;
            }
            if (c == '"' || c == '\'')
            {
                quote = true;
                quoteChar = c;
                current.Append(c);  // include opening quote ? Value() needs it
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Length = 0;
                }
                continue;
            }
            current.Append(c);
        }

        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }

    static string Join(List<string> parts, int start, int end)
    {
        var builder = new StringBuilder();
        for (int i = start; i < end; i++)
        {
            if (i > start) builder.Append(' ');
            builder.Append(parts[i]);
        }
        return builder.ToString();
    }

    static string Format(object value)
    {
        if (value == null) return "nulo";
        if (value is bool) return ((bool)value) ? "verdadero" : "falso";
        if (value is AgentPlan) return ((AgentPlan)value).ToIanText();
        if (value is List<object>)
        {
            var lst = (List<object>)value;
            return "[" + string.Join(", ", lst.ConvertAll(item => Format(item)).ToArray()) + "]";
        }
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    static string ToJson(object value)
    {
        if (value == null) return "null";
        if (value is bool) return ((bool)value) ? "true" : "false";
        if (value is int || value is long || value is float || value is double || value is decimal)
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        if (value is List<object>)
        {
            var lst = (List<object>)value;
            return "[" + string.Join(", ", lst.ConvertAll(item => ToJson(item)).ToArray()) + "]";
        }
        return "\"" + Escape(Convert.ToString(value, CultureInfo.InvariantCulture)) + "\"";
    }

    static bool TcpOpen(string host, int port)
    {
        try
        {
            using (var client = new TcpClient())
            {
                IAsyncResult result = client.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                if (!success) return false;
                client.EndConnect(result);
                return true;
            }
        }
        catch { return false; }
    }

    string SafePath(string relative)
    {
        string target = Path.GetFullPath(Path.Combine(baseDir, relative));
        if (!target.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase)) throw new IanException("La salida debe quedar dentro de la carpeta del programa");
        return target;
    }

    static string RenderPage(Page page)
    {
        var css = new StringBuilder();
        css.Append(@"*{box-sizing:border-box;margin:0;padding:0}
body{font-family:'Segoe UI',system-ui,sans-serif;font-size:16px;line-height:1.6;color:#1a202c;background:#f8fafc;padding:32px 16px}
.ian-container{max-width:900px;margin:0 auto;background:#fff;border-radius:12px;box-shadow:0 2px 16px rgba(0,0,0,.08);padding:40px 48px}
.ian-h1{font-size:2rem;font-weight:700;color:#111827;margin-bottom:12px;padding-bottom:12px;border-bottom:2px solid #e5e7eb}
.ian-h2{font-size:1.4rem;font-weight:600;color:#374151;margin:24px 0 10px}
.ian-p{color:#374151;margin-bottom:16px}
.ian-ul{padding-left:24px;margin-bottom:16px;color:#374151}
.ian-ul li{margin-bottom:6px}
.ian-img{max-width:100%;height:auto;border-radius:8px;margin:16px 0;display:block}
.ian-link{color:#2563eb;text-decoration:none;font-weight:500}.ian-link:hover{text-decoration:underline}
.ian-btn{display:inline-block;padding:10px 20px;background:#111827;color:#fff;border:none;border-radius:8px;font-size:15px;cursor:pointer;margin:8px 4px;transition:background .2s}.ian-btn:hover{background:#374151}
.ian-table{width:100%;border-collapse:collapse;margin:20px 0;font-size:15px}
.ian-table th{background:#111827;color:#fff;padding:11px 14px;text-align:left;font-weight:600;font-size:14px}
.ian-table td{padding:10px 14px;border-bottom:1px solid #e5e7eb;color:#374151}
.ian-table tr:nth-child(even) td{background:#f9fafb}
.ian-table tr:hover td{background:#eff6ff}
.ian-form{background:#f9fafb;border:1px solid #e5e7eb;border-radius:10px;padding:24px;margin:20px 0}
.ian-campo{margin-bottom:16px}.ian-campo label{display:block;font-size:14px;font-weight:600;color:#374151;margin-bottom:6px}
.ian-campo input{width:100%;padding:10px 14px;border:1px solid #d1d5db;border-radius:8px;font-size:15px;outline:none;transition:border .2s}.ian-campo input:focus{border-color:#2563eb}
.ian-seccion{margin:20px 0;padding:20px;border-left:4px solid #2563eb;background:#eff6ff;border-radius:0 8px 8px 0}
@media print{body{background:#fff;padding:0}.ian-container{box-shadow:none;border-radius:0;padding:20px}}");
        if (page.CustomCss.Count > 0)
            css.Append("\n").Append(string.Join("\n", page.CustomCss.ToArray()));

        return "<!doctype html>\n<html lang=\"es\">\n<head>\n" +
               "  <meta charset=\"utf-8\">\n" +
               "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n" +
               "  <title>" + Html(page.Title) + "</title>\n" +
               "  <style>" + css.ToString() + "</style>\n" +
               "</head>\n<body>\n<div class=\"ian-container\">\n  " +
               string.Join("\n  ", page.Parts.ToArray()) +
               "\n</div>\n</body>\n</html>\n";
    }

    static string Html(string text)
    {
        return (text ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }

    static string Escape(string text)
    {
        return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'");
    }

    static string QuoteList(IPAddress[] addresses)
    {
        var parts = new List<string>();
        foreach (IPAddress address in addresses) parts.Add("\"" + Escape(address.ToString()) + "\"");
        return string.Join(", ", parts.ToArray());
    }

    static int RunProcess(string file, string args, string input)
    {
        string output;
        return RunProcessCapture(file, args, input, Directory.GetCurrentDirectory(), out output);
    }

    static int RunProcessCapture(string file, string args, string input, string workingDirectory, out string output)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = file;
        psi.Arguments = args;
        psi.UseShellExecute = false;
        psi.WorkingDirectory = workingDirectory;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        using (var process = Process.Start(psi))
        {
            process.StandardInput.Write(input ?? "");
            process.StandardInput.Close();
            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error)) output = error;
            return process.ExitCode;
        }
    }

    static string QuoteArg(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
    }
}

class IanException : Exception
{
    public IanException(string message) : base(message) { }
}

class Page
{
    public string Title;
    public List<string> Parts = new List<string>();
    public List<string> CustomCss = new List<string>();
    public StringBuilder TableBuf = null;
    public Page(string title) { Title = title; }
}

class Spreadsheet
{
    public string SheetName;
    string[] Header;
    readonly List<string[]> Rows = new List<string[]>();

    public Spreadsheet(string name) { SheetName = string.IsNullOrWhiteSpace(name) ? "Hoja1" : name; }

    public void SetHeader(string[] cols) { Header = cols; }

    public void AddRow(string[] vals) { Rows.Add(vals); }

    public string ToSpreadsheetML()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.AppendLine(" <Styles>");
        sb.AppendLine("  <Style ss:ID=\"H\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#111827\" ss:Pattern=\"Solid\"/><Font ss:Color=\"#FFFFFF\" ss:Bold=\"1\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"N\"><Alignment ss:Vertical=\"Center\"/></Style>");
        sb.AppendLine(" </Styles>");
        sb.AppendLine(" <Worksheet ss:Name=\"" + XmlAttr(SheetName) + "\">");
        sb.AppendLine("  <Table>");
        if (Header != null && Header.Length > 0)
        {
            sb.Append("   <Row ss:StyleID=\"H\">");
            foreach (string col in Header)
                sb.Append("<Cell><Data ss:Type=\"String\">").Append(XmlText(col.Trim())).Append("</Data></Cell>");
            sb.AppendLine("</Row>");
        }
        foreach (string[] row in Rows)
        {
            sb.Append("   <Row ss:StyleID=\"N\">");
            foreach (string val in row)
            {
                string v = val.Trim();
                double num;
                string type = double.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out num) ? "Number" : "String";
                sb.Append("<Cell><Data ss:Type=\"").Append(type).Append("\">").Append(XmlText(v)).Append("</Data></Cell>");
            }
            sb.AppendLine("</Row>");
        }
        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");
        sb.AppendLine("</Workbook>");
        return sb.ToString();
    }

    static string XmlText(string t) { return (t ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"); }
    static string XmlAttr(string t) { return XmlText(t).Replace("\"", "&quot;"); }
}

class WordDoc
{
    public string Title;
    readonly List<string> Blocks = new List<string>();
    StringBuilder TableBuf = null;
    int TableCols = 0;

    public WordDoc(string title) { Title = title; }

    public void AddHeading(string text, int level)
    {
        int fs = level == 1 ? 40 : 28;
        Blocks.Add("{\\pard\\sb240\\sa120\\b\\fs" + fs + " " + RtfEncode(text) + "\\b0\\par}");
    }

    public void AddParagraph(string text)
    {
        Blocks.Add("{\\pard\\sa160\\fs24 " + RtfEncode(text) + "\\par}");
    }

    public void StartTable(string[] cols)
    {
        TableCols = cols.Length;
        TableBuf = new StringBuilder();
        // Header row
        int cellW = 8640 / Math.Max(1, cols.Length);
        TableBuf.Append("{\\trowd\\trgaph108\\trleft0");
        for (int i = 0; i < cols.Length; i++)
            TableBuf.Append("\\clbrdrt\\brdrs\\clbrdrb\\brdrs\\clbrdrl\\brdrs\\clbrdrr\\brdrs\\cellx").Append((i + 1) * cellW);
        TableBuf.Append("\\pard\\intbl\\b\\fs22 ");
        for (int i = 0; i < cols.Length; i++)
        {
            if (i > 0) TableBuf.Append("\\cell\\pard\\intbl\\b\\fs22 ");
            TableBuf.Append(RtfEncode(cols[i].Trim()));
        }
        TableBuf.Append("\\b0\\cell\\row}");
    }

    public void AddTableRow(string[] vals)
    {
        if (TableBuf == null) throw new Exception("Usa 'word tabla' antes de 'word fila'");
        int cellW = 8640 / Math.Max(1, TableCols);
        TableBuf.Append("{\\trowd\\trgaph108\\trleft0");
        for (int i = 0; i < TableCols; i++)
            TableBuf.Append("\\clbrdrt\\brdrs\\clbrdrb\\brdrs\\clbrdrl\\brdrs\\clbrdrr\\brdrs\\cellx").Append((i + 1) * cellW);
        TableBuf.Append("\\pard\\intbl\\fs22 ");
        for (int i = 0; i < TableCols; i++)
        {
            if (i > 0) TableBuf.Append("\\cell\\pard\\intbl\\fs22 ");
            string val = i < vals.Length ? vals[i].Trim() : "";
            TableBuf.Append(RtfEncode(val));
        }
        TableBuf.Append("\\cell\\row}");
    }

    public void EndTable()
    {
        if (TableBuf != null)
        {
            Blocks.Add(TableBuf.ToString());
            TableBuf = null;
        }
    }

    public string ToRtf()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{\\rtf1\\ansi\\deff0");
        sb.AppendLine("{\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\froman Times New Roman;}}");
        sb.AppendLine("{\\colortbl ;\\red17\\green24\\blue39;}");
        // Title
        sb.AppendLine("{\\pard\\sb0\\sa240\\b\\fs44\\cf1 " + RtfEncode(Title) + "\\b0\\par}");
        foreach (string block in Blocks)
            sb.AppendLine(block);
        sb.Append("}");
        return sb.ToString();
    }

    static string RtfEncode(string text)
    {
        if (text == null) return "";
        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (c == '\\' || c == '{' || c == '}') { sb.Append('\\'); sb.Append(c); }
            else if (c < 128) sb.Append(c);
            else sb.Append("\\u").Append((int)c).Append("?");
        }
        return sb.ToString();
    }
}

class AgentStep
{
    public string Status;
    public string Action;
    public AgentStep(string status, string action) { Status = status; Action = action; }
}

class AgentPlan
{
    public string Goal;
    public List<AgentStep> Steps = new List<AgentStep>();
    public AgentPlan(string goal) { Goal = goal; }

    public string ToIanText()
    {
        var builder = new StringBuilder();
        builder.Append("{objetivo: \"").Append(EscapeLocal(Goal)).Append("\", pasos: [");
        for (int i = 0; i < Steps.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append("{estado: \"").Append(EscapeLocal(Steps[i].Status)).Append("\", accion: \"").Append(EscapeLocal(Steps[i].Action)).Append("\"}");
        }
        builder.Append("]}");
        return builder.ToString();
    }

    static string EscapeLocal(string text)
    {
        return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

class Bridge
{
    public string Program;
    public string Arguments;

    public Bridge(string command)
    {
        List<string> parts = SplitCommand(command);
        if (parts.Count == 0) throw new IanException("El nexo necesita un comando");
        Program = parts[0];
        Arguments = "";
        if (parts.Count > 1)
        {
            var builder = new StringBuilder();
            for (int i = 1; i < parts.Count; i++)
            {
                if (i > 1) builder.Append(' ');
                builder.Append("\"").Append(parts[i].Replace("\"", "\\\"")).Append("\"");
            }
            Arguments = builder.ToString();
        }
    }

    static List<string> SplitCommand(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool quote = false;
        char quoteChar = '\0';
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quote)
            {
                if (c == quoteChar) quote = false;
                else current.Append(c);
                continue;
            }
            if (c == '"' || c == '\'') { quote = true; quoteChar = c; continue; }
            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0) { result.Add(current.ToString()); current.Length = 0; }
                continue;
            }
            current.Append(c);
        }
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }
}

class IanFunction
{
    public string Name;
    public List<string> Arguments;
    public string[] BodyLines;
    public IanFunction(string name, List<string> args, string[] body)
    {
        Name = name;
        Arguments = args;
        BodyLines = body;
    }
}


