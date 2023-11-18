using Leaf.xNet;
using Newtonsoft.Json;

namespace Core.Discord;

public static class DiscordChecker
{
    public static DiscordAccount? TryLogin(string token)
    {
        using var request = CreateRequest(token);
        
        var response = request.Get("https://discord.com/api/v9/users/@me");

        if (response is not { StatusCode: HttpStatusCode.OK }) return null;

        var account = JsonConvert.DeserializeObject<DiscordAccount>(response.ToString()!);

        if (account is not null) account.Token = token;
        
        return account;
    }

    public static IEnumerable<DiscordFriend>? Friends(string token)
    {
        using var request = CreateRequest(token);
        
        var friends = request.Get("https://discord.com/api/v8/users/@me/relationships");

        if (friends is not { StatusCode: HttpStatusCode.OK }) return null;

        return JsonConvert.DeserializeObject<List<DiscordFriend>>(friends.ToString()!);
    }

    private static HttpRequest CreateRequest(string token)
    {
        var request = new HttpRequest();
        request.UserAgentRandomize();

        request.Proxy = new HttpProxyClient("46.8.107.43", 1050, "WGtC9e", "fRqZn7MaIS"); // todo change on proxies from file
        request.IgnoreProtocolErrors = true;
        request.Authorization = token;

        return request;
    }
}

public record DiscordFriend(
    [JsonProperty("id")] string Id,
    [JsonProperty("type")] int Type,
    [JsonProperty("nickname")] object Nickname,
    [JsonProperty("user")] DiscordAccount User,
    [JsonProperty("since")] DateTime Since
);

public class DiscordAccount
{
    public string Token { get; set; }
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("username")] public string Username { get; set; }
    [JsonProperty("avatar")] public string Avatar { get; set; }
    [JsonProperty("discriminator")] public string Discriminator { get; set; }
    [JsonProperty("public_flags")] public int PublicFlags { get; set; }
    [JsonProperty("premium_type")] public int PremiumType { get; set; }
    [JsonProperty("flags")] public int Flags { get; set; }
    [JsonProperty("banner")] public object Banner { get; set; }
    [JsonProperty("accent_color")] public int? AccentColor { get; set; }
    [JsonProperty("global_name")] public string GlobalName { get; set; }
    [JsonProperty("avatar_decoration_data")] public object AvatarDecorationData { get; set; }
    [JsonProperty("banner_color")] public string BannerColor { get; set; }
    [JsonProperty("mfa_enabled")] public bool MfaEnabled { get; set; }
    [JsonProperty("locale")] public string Locale { get; set; }
    [JsonProperty("email")] public string Email { get; set; }
    [JsonProperty("verified")] public bool Verified { get; set; }
    [JsonProperty("phone")] public string Phone { get; set; }
    [JsonProperty("nsfw_allowed")] public bool? NsfwAllowed { get; set; }
    [JsonProperty("linked_users")] public IReadOnlyList<object> LinkedUsers { get; set; }
    [JsonProperty("bio")] public string Bio { get; set; }
    [JsonProperty("authenticator_types")] public IReadOnlyList<object> AuthenticatorTypes { get; set; }
}