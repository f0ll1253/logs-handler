using Core.Wallets.Abstractions;

namespace Core.Wallets;

public class WalletMetamask : IWallet
{
    public required string Log { get; init; }
    public required string Mnemonic { get; init; }
    public string? Address { get; init; } = "";
}