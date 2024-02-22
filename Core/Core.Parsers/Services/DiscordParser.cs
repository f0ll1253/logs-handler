namespace Core.Parsers.Services;

public static class DiscordParser
{
    public static IEnumerable<string> DiscordByLogs(this string logs) =>
        Directory.GetDirectories(logs).SelectMany(DiscordParser.DiscordByLog);

    public static IEnumerable<string> DiscordByLog(this string log)
    {
        string file = Path.Combine(log, "Discord", "Tokens.txt");
        if (!File.Exists(file)) yield break;

        using var reader = new StreamReader(file);

        while (!reader.EndOfStream)
        {
            if (reader.ReadLine() is not { } line || string.IsNullOrEmpty(line)) continue;

            yield return line;
        }
    }
}