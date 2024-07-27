using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class AvatarDataClass {
		[JsonProperty("sku_id")]
		public string SkuId { get; set; }
			
		[JsonProperty("asset")]
		public string Asset { get; set; }
	}
}