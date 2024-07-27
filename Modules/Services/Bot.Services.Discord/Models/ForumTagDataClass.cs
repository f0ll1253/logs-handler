using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ForumTagDataClass {
		[JsonProperty("id")]
		public string Id { get; set; }
			
		[JsonProperty("name")]
		public string Name { get; set; }
			
		[JsonProperty("moderated")]
		public bool Moderated { get; set; }
			
		[JsonProperty("emoji_id")]
		public string? EmojiId { get; set; }
			
		[JsonProperty("emoji_name")]
		public string? EmojiName { get; set; }
	}
}