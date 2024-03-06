using System.Text.RegularExpressions;
using Core.Models.Extensions;

namespace Core.Parsers.Services;

public static partial class DiscordParser
{
    public static IEnumerable<string> DiscordByLogs(this string logs) =>
        Directory.GetDirectories(logs)
            .SelectPerTask(DiscordByLog)
            .SelectMany(x => x);

    public static IEnumerable<string> DiscordByLog(this string log)
    {
        var type = GetLogType(log, out var filepath, out var exists);
        
        if (!exists) return ArraySegment<string>.Empty;
        
        switch (type)
        {
            case LogType.Redline:
                return RedlineDiscordByLog(filepath);
            
            case LogType.Risepro:
                return RiseproDiscordByLog(filepath);
            
            default:
                return ArraySegment<string>.Empty;
        }
    }

    private static IEnumerable<string> RedlineDiscordByLog(string filepath)
    {
        using var reader = new StreamReader(filepath);

        while (!reader.EndOfStream)
        {
            yield return reader.ReadLine()!;
        }
    }

    [GeneratedRegex(@"Token: (.*?)($|\n)")]
    private static partial Regex RiseproToken();
    
    private static IEnumerable<string> RiseproDiscordByLog(string filepath)
    {
        using var reader = new StreamReader(filepath);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;

            if (!line.StartsWith("Token: ")) continue;
            
            yield return RiseproToken().Match(line).Groups[1].Value;
        }
    }

    private static LogType GetLogType(string log, out string filepath, out bool exists)
    {
        exists = true;
        
        filepath = Path.Combine(log, "Discord", "Tokens.txt");
        
        if (File.Exists(filepath)) return LogType.Redline;

        filepath = Path.Combine(log, "discord.txt");
        
        if (File.Exists(filepath)) return LogType.Risepro;

        exists = false;
        
        return LogType.Unknown;
    }
    
    private enum LogType
    {
        Redline,
        Risepro,
        Unknown
    }
}