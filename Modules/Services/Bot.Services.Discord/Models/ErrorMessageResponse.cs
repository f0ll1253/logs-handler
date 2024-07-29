using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	public class ErrorMessageResponse : Exception {
		[JsonProperty("message")]
		public new string Message { get; set; }
		
		[JsonProperty("code")]
		public int Code { get; set; }
	}
}