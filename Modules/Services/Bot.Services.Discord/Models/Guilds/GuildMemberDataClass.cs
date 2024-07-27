using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Guilds {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class GuildMemberDataClass {
		[JsonProperty("user")]
		public User User { get; set; }
			
		[JsonProperty("nick")]
		public string? Nick { get; set; }
			
		[JsonProperty("avatar")]
		public string AvatarHash { get; set; }

		[JsonProperty("roles")]
		public ICollection<string> Roles { get; set; } = [];
			
		[JsonProperty("joined_at")]
		public DateTimeOffset JoinedAt { get; set; }
			
		[JsonProperty("premium_since")]
		public DateTimeOffset? PremiumSince { get; set; }
			
		[JsonProperty("deaf")]
		public bool Deaf { get; set; }
			
		[JsonProperty("mute")]
		public bool Mute { get; set; }
			
		[JsonProperty("flags")]
		public int Flags { get; set; }
			
		[JsonProperty("pending")]
		public bool? Pending { get; set; }
			
		[JsonProperty("permissions")]
		public string? Permissions { get; set; }
			
		[JsonProperty("communication_disabled_until")]
		public DateTimeOffset? CommunicationDisabledUntil { get; set; }
			
		[JsonProperty("avatar_decoration_data")]
		public AvatarDataClass? AvatarData { get; set; }
	}
}