namespace Bot.Services.Twitch.Models {
	public class User {
		public string Token { get; set; }

		public static implicit operator User(string str) => new() {
			Token = str
		};
	}
}