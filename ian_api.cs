using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

class IanApi
{
    const string Version = "0.1.0";
    static readonly string Home = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string ApiDir = Path.Combine(Home, "apis");
    static readonly string SourcesFile = Path.Combine(ApiDir, "sources.tsv");

    public static int Run(string[] args)
    {
        Directory.CreateDirectory(ApiDir);

        if (args.Length == 0 || args[0] == "--help")
        {
            Help();
            return 0;
        }
        if (args[0] == "--version")
        {
            Console.WriteLine("i@N API " + Version);
            return 0;
        }

        try
        {
            string cmd = args[0].ToLowerInvariant();
            if (cmd == "agregar") return Add(args);
            if (cmd == "listar") return List();
            if (cmd == "preguntar") return Ask(args);
            if (cmd == "probar") return Test(args);
            if (cmd == "ejemplo") return Example();
            throw new Exception("Comando desconocido: " + args[0]);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("i@N API error: " + ex.Message);
            return 1;
        }
    }

    static void Help()
    {
        Console.WriteLine("i@N API " + Version);
        Console.WriteLine("Uso:");
        Console.WriteLine("  ian-api ejemplo");
        Console.WriteLine("  ian-api agregar nombre \"https://api.ejemplo.com/buscar?q={pregunta}\"");
        Console.WriteLine("  ian-api listar");
        Console.WriteLine("  ian-api preguntar nombre \"consulta\"");
        Console.WriteLine("  ian-api probar nombre");
        Console.WriteLine();
        Console.WriteLine("La URL debe usar {pregunta} donde i@N pondra la consulta codificada.");
    }

    static int Add(string[] args)
    {
        if (args.Length < 3) throw new Exception("Uso: agregar nombre \"url con {pregunta}\"");
        string name = CleanName(args[1]);
        string url = string.Join(" ", Slice(args, 2)).Trim();
        if (!url.Contains("{pregunta}")) throw new Exception("La URL debe contener {pregunta}");

        var sources = Load();
        sources[name] = url;
        Save(sources);
        Console.WriteLine("API agregada: " + name);
        return 0;
    }

    static int List()
    {
        var sources = Load();
        if (sources.Count == 0)
        {
            Console.WriteLine("No hay APIs configuradas.");
            Console.WriteLine("Usa: ian-api ejemplo");
            return 0;
        }
        foreach (var item in sources)
            Console.WriteLine(item.Key + " -> " + item.Value);
        return 0;
    }

    static int Ask(string[] args)
    {
        if (args.Length < 3) throw new Exception("Uso: preguntar nombre \"consulta\"");
        string name = CleanName(args[1]);
        string question = string.Join(" ", Slice(args, 2));
        var sources = Load();
        if (!sources.ContainsKey(name)) throw new Exception("API no configurada: " + name);
        Console.WriteLine(Query(sources[name], question));
        return 0;
    }

    static int Test(string[] args)
    {
        if (args.Length < 2) throw new Exception("Uso: probar nombre");
        string name = CleanName(args[1]);
        var sources = Load();
        if (!sources.ContainsKey(name)) throw new Exception("API no configurada: " + name);
        Console.WriteLine(Query(sources[name], "prueba"));
        return 0;
    }

    static int Example()
    {
        Console.WriteLine("Ejemplos de conexion:");
        Console.WriteLine();
        Console.WriteLine("Wikipedia resumen:");
        Console.WriteLine("ian-api agregar wiki \"https://es.wikipedia.org/api/rest_v1/page/summary/{pregunta}\"");
        Console.WriteLine("ian-api preguntar wiki \"MikroTik\"");
        Console.WriteLine();
        Console.WriteLine("API propia:");
        Console.WriteLine("ian-api agregar miapi \"https://mi-servidor.local/buscar?q={pregunta}\"");
        Console.WriteLine("ian-api preguntar miapi \"estado del cliente 123\"");
        return 0;
    }

    static string Query(string template, string question)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        string url = template.Replace("{pregunta}", Uri.EscapeDataString(question));
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.UserAgent = "i@N-API/0.1";
        request.Timeout = 15000;

        using (var response = (HttpWebResponse)request.GetResponse())
        using (var stream = response.GetResponseStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            string text = reader.ReadToEnd().Trim();
            string extract = ExtractJsonString(text, "extract");
            if (!string.IsNullOrWhiteSpace(extract)) return extract;
            if (text.Length > 3000) text = text.Substring(0, 3000) + "\n... respuesta recortada ...";
            return text;
        }
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
            if (c == '\\')
            {
                escape = true;
                continue;
            }
            if (c == '"') break;
            builder.Append(c);
        }
        return builder.ToString();
    }

    static Dictionary<string, string> Load()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(SourcesFile)) return map;
        foreach (string line in File.ReadAllLines(SourcesFile, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split('\t');
            if (parts.Length >= 2) map[parts[0]] = parts[1];
        }
        return map;
    }

    static void Save(Dictionary<string, string> sources)
    {
        var lines = new List<string>();
        foreach (var item in sources)
            lines.Add(item.Key + "\t" + item.Value);
        File.WriteAllLines(SourcesFile, lines.ToArray(), Encoding.UTF8);
    }

    static string CleanName(string value)
    {
        value = (value ?? "").Trim().ToLowerInvariant();
        if (value.Length == 0) throw new Exception("Nombre vacio");
        foreach (char c in value)
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                throw new Exception("Nombre invalido. Usa letras, numeros, guion o guion bajo.");
        return value;
    }

    static string[] Slice(string[] input, int start)
    {
        if (start >= input.Length) return new string[0];
        string[] output = new string[input.Length - start];
        for (int i = start; i < input.Length; i++) output[i - start] = input[i];
        return output;
    }
}

