namespace Bot.Services.Database.Models.Abstractions {
	public interface ICredentials {
		public string Protocol { get; set; }
		public string Domain { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}