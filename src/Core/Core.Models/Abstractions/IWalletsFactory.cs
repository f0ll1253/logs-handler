using Core.Models.Abstractions;

namespace Core.Wallets.Abstractions;

public interface IWalletsFactory<T>
    where T : IWallet
{
    IAsyncEnumerable<T> Create(ILog log);
}