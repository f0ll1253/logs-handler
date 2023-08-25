using Newtonsoft.Json;

namespace logs_handler;

[JsonObject(MemberSerialization.OptOut)]
public class Config
{
    [JsonProperty("ConnectionStrings")] public Dictionary<string, string> ConnectionStrings { get; set; } = new();
}