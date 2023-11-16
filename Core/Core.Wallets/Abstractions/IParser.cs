namespace Core.Wallets.Abstractions;

public interface IParser
{
    IEnumerable<IWallet?> ByLogs(string logs);
    IEnumerable<IWallet?> ByLog(string log);
    IWallet? ByWallet(string dir, IEnumerable<string> passwords);
}