namespace Bot.Core.Models.Proxies.Abstractions {
	public interface IProxy {
		string Id { get; set; }
		
		string Host { get; set; }
		uint Port { get; set; }
		string Username { get; set; }
		string Password { get; set; }
	}
}