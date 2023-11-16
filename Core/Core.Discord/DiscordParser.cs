namespace Core.Discord;

public class DiscordParser
{
    public IEnumerable<string> ByLogs(string logs) => Directory.GetDirectories(logs).SelectMany(ByLog);
    
    public IEnumerable<string> ByLog(string log)
    {
        if (!Directory.Exists(Path.Combine(log, "Discord"))) yield break;

        using var reader = new StreamReader(Path.Combine(log, "Discord", "Tokens.txt"));

        while (!reader.EndOfStream)
        {
            if (reader.ReadLine() is not {} line || string.IsNullOrEmpty(line)) continue;

            yield return line;
        }
    }
}