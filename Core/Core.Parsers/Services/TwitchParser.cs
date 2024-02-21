using System.Text.RegularExpressions;
using Core.Models.Extensions;

namespace Core.Parsers.Services;

public static class TwitchParser
{
    public static IEnumerable<string> TwitchFromLogs(this string logspath)
        => Directory.GetDirectories(logspath)
            .SelectPerTask(TwitchFromLog)
            .SelectMany(x => x);
    
    public static IEnumerable<string> TwitchFromLog(this string logpath)
    {
        logpath = Path.Combine(logpath, "Cookies");
        
        if (!Directory.Exists(logpath)) yield break;

        foreach (var cookiesfile in Directory.GetFiles(logpath))
        {
            var token = cookiesfile.TwitchFromCookies();
            
            if (token is null) continue;

            yield return token;
        }
    }
    
    private static readonly Regex _token = new(@"passport\.twitch\.tv\t.*?(abel|abel-ssn)\t(.*?)($|\n)");
    public static string? TwitchFromCookies(this string filepath)
    {
        if (!File.Exists(filepath)) return null;
        
        using var reader = new StreamReader(filepath);
        string? token = null;

        while (token is null && !reader.EndOfStream)
        {
            var line = reader.ReadLine();
            
            if (string.IsNullOrEmpty(line) || !line.StartsWith("passport.twitch.tv")) continue;

            var match = _token.Match(line);
            
            if (!match.Groups[2].Success) continue;

            token = match.Groups[2].Value;
        }

        return token;
    }
}