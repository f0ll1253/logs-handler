namespace Core.Models.Abstractions;

public interface ILogsFactory
{
    Task<ILog> Create(string path);
    IAsyncEnumerable<IPasswordField> ParsePasswords(string path);
    bool TelegramExists(string path);
    bool SteamExists(string path);
    bool WalletsExists(string path);
}