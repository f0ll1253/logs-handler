namespace Core.Parsers;

public static class Cookies
{
    public static IDictionary<string, IEnumerable<IEnumerable<string>>> CookiesFromLogs(this IEnumerable<string> domains, string path)
    {
        if (!Directory.Exists(path)) return new Dictionary<string, IEnumerable<IEnumerable<string>>>();

        var res = new Dictionary<string, List<IEnumerable<string>>>();
        var data = Directory.GetDirectories(path).Select(domains.CookiesFromLog);

        foreach (var cookies in data)
        {
            foreach (var cookie in cookies)
            {
                if (res.TryGetValue(cookie.Key, out var list))
                    list.AddRange(cookie.Value);
                else
                {
                    var newlist = new List<IEnumerable<string>>();
                    newlist.AddRange(cookie.Value);
                    res.Add(cookie.Key, newlist);
                }
            }
        }

        return res.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }
    
    public static IDictionary<string, IEnumerable<IEnumerable<string>>> CookiesFromLog(this IEnumerable<string> domains, string path)
    {
        path = Path.Combine(path, "Cookies");

        if (!Directory.Exists(path)) return new Dictionary<string, IEnumerable<IEnumerable<string>>>();

        var res = new Dictionary<string, List<IEnumerable<string>>>();
        var data = Directory.GetFiles(path).Select(domains.CookiesFromFile);

        foreach (var cookies in data)
        {
            foreach (var cookie in cookies)
            {
                if (res.TryGetValue(cookie.Key, out var list))
                    list.Add(cookie.Value);
                else
                    res.Add(cookie.Key, new List<IEnumerable<string>> {cookie.Value});
            }
        }

        return res.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }

    public static IDictionary<string, IEnumerable<string>> CookiesFromFile(this IEnumerable<string> domains, string path)
    {
        var result = new Dictionary<string, List<string>>();
        
        if (!File.Exists(path)) return result.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());

        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var domain = line?[..line.IndexOf('\t')];
            
            if (domain is null || !domains.Contains(domain)) continue;

            result.TryAdd(domain, new List<string>());
            result[domain].Add(line!);
        }
        
        return result.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }
    
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