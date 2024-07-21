using System.Net;

using Bot.Core.Models.Proxies.Abstractions;

namespace Bot.Services.Proxies.Models {
	public class Proxy : IProxy {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		
		public required string Host { get; set; }
		public required uint Port { get; set; }
		public required string Username { get; set; }
		public required string Password { get; set; }
		public ProxyType Type { get; set; } = ProxyType.Http;

		public static explicit operator string(Proxy proxy) => $"{proxy.Host}:{proxy.Port}:{proxy.Username}:{proxy.Password}";

		public static implicit operator WebProxy(Proxy proxy) => new() {
			Address = new($"{proxy.Type}://{proxy.Host}:{proxy.Port}"),
			Credentials = new NetworkCredential(proxy.Username, proxy.Password)
		};
	}

	public enum ProxyType {
		Http,
		Https,
		Socks4,
		Socks5
	}
}