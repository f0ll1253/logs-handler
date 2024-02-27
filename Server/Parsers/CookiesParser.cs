using Server.Extensions;
using Server.Models;
using Server.Models.Abstract;

namespace Server.Parsers;

public class CookiesParser(IEnumerable<string> domains) : IMultipleParser<Cookie>
{
    public IAsyncEnumerable<Cookie> FromLogs(string logs) =>
        Directory.GetDirectories(logs)
                 .ToAsyncEnumerable()
                 .SelectMany(FromLog);

    public async IAsyncEnumerable<Cookie> FromLog(string log)
    {
        var lines = domains.ToDictionary(x => x, x => new List<string>());
        
        foreach (var file in log.GetCookieFiles())
        {
            foreach (var list in lines.Values)
                list.Clear();
            
            using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var end = line.IndexOf('\t');

                        if (end <= 0)
                            continue;

                        var domain = line[..end];

                        KeyValuePair<string, List<string>>? cookie = lines.FirstOrDefault(x => domain.Contains(x.Key));

                        cookie?.Value.Add(line);
                    }

            foreach (var (domain, list) in lines)
                yield return new Cookie(domain, list);
        }
    }
}