namespace Core.Discord;

public static class DiscordParser
{
    public static IEnumerable<string> DiscordByLogs(this string logs) => Directory.GetDirectories(logs).SelectMany(DiscordByLog);
    
    public  static IEnumerable<string> DiscordByLog(this string log)
    {
        var file = Path.Combine(log, "Discord", "Tokens.txt");
        if (!File.Exists(file)) yield break;

        using var reader = new StreamReader(file);

        while (!reader.EndOfStream)
        {
            if (reader.ReadLine() is not {} line || string.IsNullOrEmpty(line)) continue;

            yield return line;
        }
    }
}