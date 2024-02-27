using System.Collections.ObjectModel;
using Server.Extensions;
using Server.Models;
using Server.Models.Abstract;

namespace Server.Parsers;

public class CookieParser(string domain) : IMultipleParser<Cookie>
{
    public IAsyncEnumerable<Cookie> FromLogs(string logs) =>
        Directory.GetDirectories(logs)
                 .ToAsyncEnumerable()
                 .SelectMany(FromLog);

    public async IAsyncEnumerable<Cookie> FromLog(string log)
    {
        var lines = new List<string>();
        
        foreach (var file in log.GetCookieFiles())
        {
            lines.Clear();
            
            using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var end = line.IndexOf('\t');

                        if (end <= 0 && !line[..end].Contains(domain))
                            continue;

                        lines.Add(line);
                    }

            yield return new Cookie(domain, new ReadOnlyCollection<string>(lines));
        }
    }
}