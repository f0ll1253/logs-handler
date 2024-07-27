using Bot.Services.Discord.Models.Guilds;

using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class User {
		internal User() { }

		[JsonProperty("token")]
		public string Token { get; set; }

		public string AvatarSource => $"https://cdn.discordapp.com/avatars/{Id}/{AvatarId}.webp";
		
		#region @me

		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("username")]
		public string Username { get; set; }
		
		[JsonProperty("global_name")]
		public string GlobalUsername { get; set; }
		
		[JsonProperty("discriminator")]
		public string Discriminator { get; set; }
		
		[JsonProperty("avatar")]
		public string? AvatarId { get; set; }
		
		[JsonProperty("verified")]
		public bool Verified { get; set; }
		
		[JsonProperty("email")]
		public string Email { get; set; }
		
		[JsonProperty("phone")]
		public string Phone { get; set; }
		
		[JsonProperty("locale")]
		public string CountryCode { get; set; }
		
		[JsonProperty("flags")]
		public UserFlags Flags { get; set; }
		
		[JsonProperty("banner")]
		public string Banner { get; set; }
		
		[JsonProperty("accent_color")]
		public int? AccentColor { get; set; }
		
		[JsonProperty("premium_type")]
		public PremiumFlags PremiumType { get; set; }
		
		[JsonProperty("public_flags")]
		public UserFlags PublicFlags { get; set; }
		
		[JsonProperty("avatar_decoration_data")]
		public AvatarDataClass AvatarData { get; set; }

		#endregion

		[JsonProperty("guilds")]
		public ICollection<GuildDataClass> Guilds { get; set; } = [];

		[JsonProperty("channels")]
		public ICollection<ChannelDataClass> Channels { get; set; } = [];

		[JsonProperty("relationships")]
		public ICollection<RealationShipDataClass> Friends { get; set; } = [];

		public static implicit operator User(string str) => new() {
			Token = str
		};
		
		[Flags]
		public enum UserFlags {
			STAFF = 0,
			PARTNER,
			HYPESQUAD,
			BUG_HUNTER_LEVEL_1,
			HYPESQUAD_ONLINE_HOUSE_1,
			HYPESQUAD_ONLINE_HOUSE_2,
			HYPESQUAD_ONLINE_HOUSE_3,
			PREMIUM_EARLY_SUPPORTER,
			TEAM_PSEUDO_USER,
			BUG_HUNTER_LEVEL_2,
			VERIFIED_BOT,
			VERIFIED_DEVELOPER,
			CERTIFIED_MODERATOR,
			BOT_HTTP_INTERACTIONS,
			ACTIVE_DEVELOPER,
		}
		
		public enum PremiumFlags {
			None = 0,
			NitroClassic,
			Nitro,
			NitroBasic
		}
	}
}