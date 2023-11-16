using Newtonsoft.Json;

namespace Core.Wallets;

public class MetamaskWallet
{
    public WalletType Type { get; set; }
    public string? Mnemonic { get; set; }
    public string? Password { get; set; }
    public Dictionary<string, MetamaskAccount> Accounts { get; set; } = new();
}

public class MetamaskAccount
{
    [JsonProperty("address")] public string Address { get; set; } = "";
    [JsonProperty("lastSelected")] public long LastSelected { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = "";
}