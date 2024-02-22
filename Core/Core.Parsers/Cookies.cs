using System.Text.RegularExpressions;
using Core.Models;
using Core.Models.Extensions;

namespace Core.Parsers;

public static class Cookies
{
    public static IDictionary<Cookie, IEnumerable<IEnumerable<string>>> CookiesFromLogs(
        this IEnumerable<Cookie> domains, string path) =>
        Directory.GetDirectories(path)
                 .SelectPerTask(domains.CookiesFromLog)
                 .SelectMany(x => x)
                 .ToLookup(x => x.Key)
                 .ToDictionary(x => x.Key, x => x.SelectMany(x => x.Value).Where(x => x.Any()));

    public static IDictionary<Cookie, IEnumerable<IEnumerable<string>>> CookiesFromLog(
        this IEnumerable<Cookie> domains, string path)
    {
        path = Path.Combine(path, "Cookies");

        if (!Directory.Exists(path)) return new Dictionary<Cookie, IEnumerable<IEnumerable<string>>>();

        var res = new Dictionary<Cookie, List<IEnumerable<string>>>();

        foreach (var cookies in Directory.GetFiles(path).Select(domains.CookiesFromFile))
        {
            foreach (var cookie in cookies)
                if (res.TryGetValue(cookie.Key, out var list))
                    list.Add(cookie.Value);
                else
                    res.Add(cookie.Key, [cookie.Value]);
        }

        return res.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }

    public static IDictionary<Cookie, IEnumerable<string>> CookiesFromFile(
        this IEnumerable<Cookie> cookies, string path)
    {
        if (!File.Exists(path)) return new Dictionary<Cookie, IEnumerable<string>>();

        // parsing
        var result = new Dictionary<Cookie, List<string>>();
        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();

            if (line is not { Length: > 1 }) continue;

            int index = line.IndexOf('\t');
            string domain = line[..(index == -1 ? 1 : index)];

            if (
                cookies.FirstOrDefault(
                    x => x.IsFull
                        ? x.Domains.FirstOrDefault(a => domain == $".{a}") is { }
                        : x.Domains.FirstOrDefault(a => domain.Contains(a)) is { }) is not
                { } pickedCookie) continue;

            result.TryAdd(pickedCookie, []);
            result[pickedCookie].Add(line);
        }

        // validators
        foreach ((var cookie, var list) in result)
        {
            bool domainspassed = false;
            var passdomains = cookie.Domains.ToDictionary(x => x, _ => false);
            var passrequires = cookie.Require.ToDictionary(x => x, _ => false);

            // check that all domains contains in result
            foreach (string? domain in
                     from domain in cookie.Domains
                     from cookieline in list.Where(x => x.Contains(domain))
                     select domain)
                passdomains[domain] = true;

            foreach ((string domain, bool value) in passdomains)
            {
                if (!value)
                {
                    result.Remove(cookie);
                    break;
                }

                domainspassed = true;
            }

            if (!domainspassed) continue;

            // checks requirement cookies
            foreach (var require in
                     from require in cookie.Require.Select(x => new Regex(x))
                     from cookieline in list.Where(cookieline => require.IsMatch(cookieline))
                     select require)
                passrequires[require.ToString()] = true;

            foreach ((string require, bool value) in passrequires)
                if (!value)
                {
                    result.Remove(cookie);
                    break;
                }
        }

        return result.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }
}