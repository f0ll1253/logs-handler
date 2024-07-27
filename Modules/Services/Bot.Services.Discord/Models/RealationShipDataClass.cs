using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	public class RealationShipDataClass {
		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("type")]
		public int Type { get; set; }
		
		[JsonProperty("nickname")]
		public string? Nickname { get; set; }
		
		[JsonProperty("user")]
		public User User { get; set; }

		[JsonProperty("since")]
		public DateTimeOffset Since { get; set; }
	}
}