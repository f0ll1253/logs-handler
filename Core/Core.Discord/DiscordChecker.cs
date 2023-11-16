using Leaf.xNet;
using Newtonsoft.Json;

namespace Core.Discord;

public class DiscordChecker : IDisposable
{
    private readonly HttpRequest _request;
    
    public DiscordChecker()
    {
        _request = new HttpRequest();
        _request.UserAgentRandomize();

        _request.Proxy = new HttpProxyClient("46.8.107.43", 1050, "WGtC9e", "fRqZn7MaIS"); // todo change on proxies from file
        _request.IgnoreProtocolErrors = true;
    }

    public void Dispose()
    {
        _request.Dispose();
    }

    public DiscordAccount? TryLogin(string token)
    {
        _request.Authorization = token;

        var userInfo = _request.Get("https://discord.com/api/v9/users/@me");

        if (userInfo is not { StatusCode: HttpStatusCode.OK }) return null;

        var account = JsonConvert.DeserializeObject<DiscordAccount>(userInfo.ToString()!);

        if (account is not null) account.Token = token;
        
        return account;
    }

    public IEnumerable<DiscordFriend>? Friends(DiscordAccount account)
    {
        var friends = _request.Get("https://discord.com/api/v8/users/@me/relationships");

        if (friends is not { StatusCode: HttpStatusCode.OK }) return null;

        return JsonConvert.DeserializeObject<List<DiscordFriend>>(friends.ToString()!);
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