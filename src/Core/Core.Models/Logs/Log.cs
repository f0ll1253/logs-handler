using Core.Models.Abstractions;

namespace Core.Models.Logs;

public class Log : ILog
{
    public required string Path { get; init; }
    public bool HasTelegram { get; init; }
    public bool HasSteam { get; init; }
    public bool HasWallets { get; init; }
    public IAsyncEnumerable<IPasswordField> Passwords { get; init; } = AsyncEnumerable.Empty<IPasswordField>();
}