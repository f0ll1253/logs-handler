using Leaf.xNet;
using Newtonsoft.Json;

namespace Core.Discord;

public class DiscordChecker
{
    public DiscordAccount? TryLogin(string token)
    {
        using var request = new HttpRequest();
        
        request.UserAgentRandomize();
        request.Authorization = token;
        request.IgnoreProtocolErrors = true;

        var response = request.Get("https://discord.com/api/v9/users/@me");

        if (response is not { StatusCode: HttpStatusCode.OK }) return null;
        
        return JsonConvert.DeserializeObject<DiscordAccount>(response.ToString()!);
    }
}

public record DiscordAccount(
    [JsonProperty("id")] string Id,
    [JsonProperty("username")] string Username,
    [JsonProperty("avatar")] string Avatar,
    [JsonProperty("discriminator")] string Discriminator,
    [JsonProperty("public_flags")] int PublicFlags,
    [JsonProperty("premium_type")] int PremiumType,
    [JsonProperty("flags")] int Flags,
    [JsonProperty("banner")] object Banner,
    [JsonProperty("accent_color")] int? AccentColor,
    [JsonProperty("global_name")] string GlobalName,
    [JsonProperty("avatar_decoration_data")] object AvatarDecorationData,
    [JsonProperty("banner_color")] string BannerColor,
    [JsonProperty("mfa_enabled")] bool MfaEnabled,
    [JsonProperty("locale")] string Locale,
    [JsonProperty("email")] string Email,
    [JsonProperty("verified")] bool Verified,
    [JsonProperty("phone")] string Phone,
    [JsonProperty("nsfw_allowed")] bool? NsfwAllowed,
    [JsonProperty("linked_users")] IReadOnlyList<object> LinkedUsers,
    [JsonProperty("bio")] string Bio,
    [JsonProperty("authenticator_types")] IReadOnlyList<object> AuthenticatorTypes
);