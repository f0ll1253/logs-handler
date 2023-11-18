namespace Core.Parsers;

public static class Cookies
{
    public static IEnumerable<IEnumerable<string>> FromLogs(this IEnumerable<string> domains, string path)
        => domains.SelectMany(x => x.FromLogs(path));
    
    public static IEnumerable<IEnumerable<string>> FromLog(this IEnumerable<string> domains, string path)
        => domains.SelectMany(x => x.FromLog(path));
    
    public static IEnumerable<string> FromFile(this IEnumerable<string> domains, string path)
        => domains.SelectMany(x => x.FromFile(path));
    
    public static IEnumerable<IEnumerable<string>> FromLogs(this string domain, string path)
        => !Directory.Exists(path)
            ? ArraySegment<IEnumerable<string>>.Empty
            : Directory.GetDirectories(path)
                .SelectMany(domain.FromLog)
                .Where(x => x.Any());
    
    public static IEnumerable<IEnumerable<string>> FromLog(this string domain, string path)
    {
        var cookies = Path.Combine(path, "Cookies");
        
        return !Directory.Exists(cookies)
            ? ArraySegment<IEnumerable<string>>.Empty
            : Directory.GetFiles(cookies).Select(domain.FromFile);
    }

    public static IEnumerable<string> FromFile(this string domain, string path)
    {
        if (!File.Exists(path)) yield break;

        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (line is not null && line.StartsWith(domain)) yield return line;
        }
    }
}