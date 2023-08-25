namespace Core.Models.Abstractions;

public interface ILog
{
    public string Path { get; init; }

    public bool HasTelegram { get; init; }
    public bool HasSteam { get; init; }
    public bool HasWallets { get; init; }
    
    public IAsyncEnumerable<IPasswordField> Passwords { get; init; }
}