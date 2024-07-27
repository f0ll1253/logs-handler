using Bot.Services.Discord.Models.Guilds;

using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Channels {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ThreadMemberDataClass {
		[JsonProperty("id")]
		public string? Id { get; set; }
			
		[JsonProperty("user_id")]
		public string? UserId { get; set; }
			
		[JsonProperty("join_timestamp")]
		public DateTimeOffset JoinTimestamp { get; set; }
			
		[JsonProperty("flags")]
		public int Flags { get; set; }
			
		[JsonProperty("member")]
		public GuildMemberDataClass? Member { get; set; }
	}
}