using Bot.Parsers.Abstractions;

namespace Bot.Parsers;

[RegisterScoped<IFileParser<IEnumerable<string>, string>>]
public class ParserCookies : IFileParser<IEnumerable<string>, string> {
    public IEnumerable<IAsyncEnumerable<string>> FromLogs(string logs, IEnumerable<string> input) => Directory
        .GetDirectories(logs)
        .SelectMany(x => FromLog(x, input));

    public IEnumerable<IAsyncEnumerable<string>> FromLog(string log, IEnumerable<string> input) => Directory
        .GetFiles(Path.Combine(log, "Cookies"))
        .Select(x => FromFile(x, input));

    public async IAsyncEnumerable<string> FromFile(string filepath, IEnumerable<string> domains) {
        if (new FileInfo(filepath) is not { Exists: true } info) {
            yield break;
        }
        
        using (var stream = info.OpenRead()) {
            using (var reader = new StreamReader(stream)) {
                while (!reader.EndOfStream) {
                    var line = await reader.ReadLineAsync();
                    var domain = line[..line.IndexOf('\t')];

                    if (domains.Contains(domain)) {
                        yield return line;
                    }
                }
            }
        }
    }
}