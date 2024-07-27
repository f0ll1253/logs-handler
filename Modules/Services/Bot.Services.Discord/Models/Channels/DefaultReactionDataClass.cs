using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Channels {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class DefaultReactionDataClass {
		[JsonProperty("emoji_id")]
		public string EmojiId { get; set; }
			
		[JsonProperty("emoji_name")]
		public string EmojiName { get; set; }
	}
}