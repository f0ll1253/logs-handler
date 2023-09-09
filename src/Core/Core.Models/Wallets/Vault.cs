using Newtonsoft.Json;

namespace Core.Models.Wallets;

[JsonObject(MemberSerialization.OptOut)]
public sealed class Vault
{
    [JsonProperty("data")] public string Data { get; set; } = "";
    [JsonProperty("iv")] public string Iv { get; set; } = "";
    [JsonProperty("salt")] public string Salt { get; set; } = "";
}