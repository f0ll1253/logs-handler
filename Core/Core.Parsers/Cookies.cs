namespace Core.Parsers;

public static class Cookies
{
    public static IDictionary<string, IEnumerable<IEnumerable<string>>> CookiesFromLogs(this IEnumerable<string> domains, string path)
        => domains.ToDictionary(x => x, x => x.CookiesFromLogs(path));
    
    public static IDictionary<string, IEnumerable<IEnumerable<string>>> CookiesFromLog(this IEnumerable<string> domains, string path)
        => domains.ToDictionary(x => x, x => x.CookiesFromLog(path));
    
    public static IDictionary<string, IEnumerable<string>> CookiesFromFile(this IEnumerable<string> domains, string path)
        => domains.ToDictionary(x => x, x => x.CookiesFromFile(path));
    
    public static IEnumerable<IEnumerable<string>> CookiesFromLogs(this string domain, string path)
        => !Directory.Exists(path)
            ? ArraySegment<IEnumerable<string>>.Empty
            : Directory.GetDirectories(path)
                .SelectMany(domain.CookiesFromLog)
                .Where(x => x.Any());
    
    public static IEnumerable<IEnumerable<string>> CookiesFromLog(this string domain, string path)
    {
        var cookies = Path.Combine(path, "Cookies");
        
        return !Directory.Exists(cookies)
            ? ArraySegment<IEnumerable<string>>.Empty
            : Directory.GetFiles(cookies).Select(domain.CookiesFromFile);
    }

    public static IEnumerable<string> CookiesFromFile(this string domain, string path)
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