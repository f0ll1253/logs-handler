namespace Core.Wallets.Abstractions;

public enum WalletType
{
    Metamask
}

public interface IWallet
{
    WalletType Type { get; set; }
    string? Mnemonic { get; set; }
}