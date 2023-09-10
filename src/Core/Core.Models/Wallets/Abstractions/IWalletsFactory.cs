using Core.Models.Logs.Abstractions;

namespace Core.Models.Wallets.Abstractions;

public interface IWalletsFactory<T>
    where T : IWallet
{
    IAsyncEnumerable<T> Create(ILog log);
}