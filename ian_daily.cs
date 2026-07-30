using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

class IanDaily
{
    const string Version = "0.1.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string LearnDir = Path.Combine(Home, "daily-learning");
    static readonly string ManualInbox = Path.Combine(Home, "teach-inbox", "manual");

    public static int Run(string[] args)
    {
        Directory.CreateDirectory(LearnDir);
        Directory.CreateDirectory(ManualInbox);
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        if (args.Length > 0 && args[0] == "--version")
        {
            Console.WriteLine("i@N Daily Learn " + Version);
            return 0;
        }
        if (args.Length > 0 && args[0] == "--help")
        {
            Help();
            return 0;
        }

        try
        {
            return Learn(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N Daily error: " + ex.Message);
            return 1;
        }
    }

    static void Help()
    {
        Console.WriteLine("i@N Daily Learn " + Version);
        Console.WriteLine("Uso:");
        Console.WriteLine("  ian.exe daily");
        Console.WriteLine("  ian.exe daily \"tema a aprender\"");
        Console.WriteLine();
        Console.WriteLine("Si existen GOOGLE_API_KEY y GOOGLE_CX, puede consultar Google Custom Search.");
        Console.WriteLine("Si no, aprende desde Wikipedia aleatoria en espa??ol.");
    }

    static int Learn(string[] args)
    {
        string topic = args.Length > 0 ? string.Join(" ", args) : "";
        string source = "";
        string title = "";
        string summary = "";

        string googleKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
        string googleCx = Environment.GetEnvironmentVariable("GOOGLE_CX");

        if (!string.IsNullOrWhiteSpace(googleKey) && !string.IsNullOrWhiteSpace(googleCx))
        {
            source = "Google Custom Search";
            string query = string.IsNullOrWhiteSpace(topic) ? "tecnologia redes programacion inteligencia artificial" : topic;
            string json = Get("https://www.googleapis.com/customsearch/v1?key=" + Uri.EscapeDataString(googleKey) + "&cx=" + Uri.EscapeDataString(googleCx) + "&q=" + Uri.EscapeDataString(query));
            title = ExtractJsonString(json, "title");
            summary = ExtractJsonString(json, "snippet");
            if (string.IsNullOrWhiteSpace(summary)) summary = Trim(json, 1200);
        }
        else
        {
            source = "Wikipedia";
            string json;
            if (!string.IsNullOrWhiteSpace(topic))
                json = GetWithFallback("https://es.wikipedia.org/api/rest_v1/page/summary/" + Uri.EscapeDataString(topic), "https://es.wikipedia.org/api/rest_v1/page/random/summary");
            else
                json = Get("https://es.wikipedia.org/api/rest_v1/page/random/summary");
            title = ExtractJsonString(json, "title");
            summary = ExtractJsonString(json, "extract");
        }

        if (string.IsNullOrWhiteSpace(title)) title = string.IsNullOrWhiteSpace(topic) ? "Aprendizaje diario" : topic;
        if (string.IsNullOrWhiteSpace(summary)) summary = "No se pudo extraer resumen, pero se registro la consulta.";

        string stamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        string md = Path.Combine(LearnDir, stamp + ".md");
        string teach = Path.Combine(ManualInbox, "daily_" + stamp + ".teach");

        string markdown =
            "# Aprendizaje diario i@N\n\n" +
            "- Fecha: " + DateTime.Now.ToString("s") + "\n" +
            "- Fuente: " + source + "\n" +
            "- Tema: " + title + "\n\n" +
            summary + "\n";
        File.WriteAllText(md, markdown, Encoding.UTF8);

        string code =
            "# pregunta: aprendizaje diario sobre " + SafeLine(title) + "\n" +
            "agente plan \"Aprender algo nuevo cada dia\" como plan\n" +
            "agente paso plan hacer \"Consultar fuente externa\"\n" +
            "agente paso plan hacer \"Guardar resumen en memoria local\"\n" +
            "decir " + Q("Aprendizaje diario: " + title) + "\n" +
            "decir " + Q(Trim(summary, 900)) + "\n" +
            "agente json plan como resumen\n" +
            "decir $resumen\n";
        File.WriteAllText(teach, code, Encoding.UTF8);

        Run(Path.Combine(Home, "ian.exe"), "teacher importar");
        Run(Path.Combine(Home, "ian.exe"), "brain importar " + Q(LearnDir));

        Console.WriteLine("i@N aprendio algo nuevo.");
        Console.WriteLine("Tema: " + title);
        Console.WriteLine("Fuente: " + source);
        Console.WriteLine("Archivo: " + md);
        return 0;
    }

    static string Get(string url)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.UserAgent = "i@N-Daily/0.1";
        request.Timeout = 20000;
        using (var response = (HttpWebResponse)request.GetResponse())
        using (var stream = response.GetResponseStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
            return reader.ReadToEnd();
    }

    static string GetWithFallback(string url, string fallback)
    {
        try { return Get(url); }
        catch { return Get(fallback); }
    }

    static void Run(string file, string args)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = file;
        psi.Arguments = args;
        psi.WorkingDirectory = Home;
        psi.UseShellExecute = false;
        using (var p = Process.Start(psi))
            p.WaitForExit();
    }

    static string ExtractJsonString(string json, string field)
    {
        string key = "\"" + field + "\":\"";
        int start = json.IndexOf(key, StringComparison.Ordinal);
        if (start < 0) return "";
        start += key.Length;
        var builder = new StringBuilder();
        bool escape = false;
        for (int i = start; i < json.Length; i++)
        {
            char c = json[i];
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
            if (c == '"') break;
            builder.Append(c);
        }
        return builder.ToString();
    }

    static string Trim(string text, int max)
    {
        if (text == null) return "";
        text = text.Trim();
        if (text.Length <= max) return text;
        return text.Substring(0, max).Trim() + "...";
    }

    static string SafeLine(string text)
    {
        return Trim((text ?? "").Replace("\r", " ").Replace("\n", " "), 120);
    }

    static string Q(string value)
    {
        return "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ") + "\"";
    }
}

