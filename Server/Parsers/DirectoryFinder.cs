using Server.Models.Abstract;

namespace Server.Parsers;

public class DirectoryFinder(string name) : IMultipleParser<string>
{
    public IAsyncEnumerable<string> FromLogs(string logs) =>
        Directory.GetDirectories(logs)
                 .ToAsyncEnumerable()
                 .SelectMany(FromLog);

    public IAsyncEnumerable<string> FromLog(string log) =>
        new DirectoryInfo(Path.Combine(log, name)) is { Exists: true } info
            ? info.GetDirectories()
                  .Select(x => x.FullName)
                  .ToAsyncEnumerable()
            : AsyncEnumerable.Empty<string>();
}

public sealed class TelegramFinder() : DirectoryFinder("Telegram");

public sealed class SteamFinder() : DirectoryFinder("Steam");