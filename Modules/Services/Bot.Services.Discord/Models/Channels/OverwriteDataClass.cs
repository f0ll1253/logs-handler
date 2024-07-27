using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Channels {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class OverwriteDataClass {
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("type")]
		public int Type { get; set; }
			
		[JsonProperty("allow")]
		public string Allow { get; set; }
			
		[JsonProperty("deny")]
		public string Deny { get; set; }
	}
}