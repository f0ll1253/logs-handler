using System.Text.RegularExpressions;
using Server.Models;
using Server.Models.Abstract;

namespace Server.Parsers;

public enum Provider
{
    Redline
}

public static partial class CredentialsParserFactory
{
    public static ICredentialsMultipleParser<Credentials> Create(Provider provider) =>
        provider switch
        {
            Provider.Redline => new RedlineCredentialsMultipleParser()
        };
    
    public sealed partial class RedlineCredentialsMultipleParser : ICredentialsMultipleParser<Credentials>
    {
        public IAsyncEnumerable<Credentials> FromLogs(string logs) =>
            Directory.GetDirectories(logs)
                     .ToAsyncEnumerable()
                     .SelectMany(FromLog);

        public async IAsyncEnumerable<Credentials> FromLog(string log)
        {
            var file = new FileInfo(Path.Combine(log, "Passwords.txt"));
            
            if (!file.Exists)
                yield break;
            
            using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        
                        if (!line.StartsWith("URL"))
                            continue;

                        yield return new Credentials(
                            RedlineCredentialsMultipleParser.URL().Match(line).Groups[1].Value,
                            RedlineCredentialsMultipleParser.Username().Match(await reader.ReadLineAsync()).Groups[1].Value,
                            RedlineCredentialsMultipleParser.Password().Match(await reader.ReadLineAsync()).Groups[1].Value);
                    }
        }

        [GeneratedRegex(@"URL:\s(.+)\n")]
        public static partial Regex URL();
        
        [GeneratedRegex(@"Username:\s(.+)\n")]
        public static partial Regex Username();
        
        [GeneratedRegex(@"Password:\s(.+)\n")]
        public static partial Regex Password();
    }
}