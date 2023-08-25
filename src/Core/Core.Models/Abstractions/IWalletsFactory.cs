namespace Core.Models.Abstractions;

public interface IWalletsFactory<T>
    where T : IWallet
{
    IAsyncEnumerable<T> Create(ILog log);
}