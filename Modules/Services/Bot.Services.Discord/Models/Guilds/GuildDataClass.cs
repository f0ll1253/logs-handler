using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Guilds {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
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
		public List<string> Features { get; set; } = [];
			
		[JsonProperty("approximate_member_count")]
		public uint MemberCount { get; set; }
			
		[JsonProperty("approximate_presence_count")]
		public uint PresenceCount { get; set; }
	}
}