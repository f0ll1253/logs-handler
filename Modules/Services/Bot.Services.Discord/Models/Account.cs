using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	public class Account {
		internal Account() { }

		public string Token { get; set; }

		#region @me

		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("username")]
		public string Username { get; set; }
		
		[JsonProperty("discriminator")]
		public string Discriminator { get; set; }
		
		[JsonProperty("avatar")]
		public string Avatar { get; set; }
		
		[JsonProperty("verified")]
		public bool Verified { get; set; }
		
		[JsonProperty("email")]
		public string Email { get; set; }
		
		[JsonProperty("flags")]
		public uint Flags { get; set; }
		
		[JsonProperty("banner")]
		public string Banner { get; set; }
		
		[JsonProperty("accent_color")]
		public int AccentColor { get; set; }
		
		[JsonProperty("premium_type")]
		public PremiumFlags PremiumType { get; set; }
		
		[JsonProperty("public_flags")]
		public uint PublicFlags { get; set; }
		
		[JsonProperty("avatar_decoration_data")]
		public AvatarDataClass AvatarData { get; set; }

		#endregion

		#region guilds

		public ICollection<GuildDataClass> Guilds { get; set; } = new List<GuildDataClass>();

		#endregion

		#region country code

		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		#endregion

		#region payment-sources

		// TODO

		#endregion

		public static implicit operator Account(string str) => new() {
			Token = str
		};
		
		//
		public enum UserFlags {
			// ReSharper disable InconsistentNaming
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
			// ReSharper restore InconsistentNaming
		}
		
		public enum PremiumFlags {
			None = 0,
			NitroClassic,
			Nitro,
			NitroBasic
		}
		
		public class AvatarDataClass {
			[JsonProperty("sku_id")]
			public string SkuId { get; set; }
			
			[JsonProperty("asset")]
			public string Asset { get; set; }
		}
		
		public class GuildDataClass {
			[JsonProperty("id")]
			public string Id { get; set; }
			
			[JsonProperty("name")]
			public string Name { get; set; }
			
			[JsonProperty("icon")]
			public string Icon { get; set; }
			
			[JsonProperty("owner")]
			public bool IsOwner { get; set; }
			
			[JsonProperty("permissions")]
			public string Permissions { get; set; }

			[JsonProperty("features")]
			public List<string> Features { get; set; } = new();
			
			[JsonProperty("approximate_member_count")]
			public uint MemberCount { get; set; }
			
			[JsonProperty("approximate_presence_count")]
			public uint PresenceCount { get; set; }
		}
	}
}