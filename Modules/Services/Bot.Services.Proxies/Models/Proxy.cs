using System.Net;

using Bot.Core.Models.Proxies.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Proxies.Models {
	public class Proxy : IProxy, IAsyncDisposable {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		
		public required string Host { get; set; }
		public required uint Port { get; set; }
		public required string Username { get; set; }
		public required string Password { get; set; }
		
		public ProxyType Type { get; set; } = ProxyType.Http;
		public int Index { get; set; }
		public bool IsInUse { get; set; }

		//
		internal DbContext Context { get; init; }
		
		#region Imlicit

		public static implicit operator string(Proxy proxy) => $"{proxy.Host}:{proxy.Port}:{proxy.Username}:{proxy.Password}";

		public static implicit operator WebProxy(Proxy proxy) => new() {
			Address = new($"{proxy.Type}://{proxy.Host}:{proxy.Port}"),
			Credentials = new NetworkCredential(proxy.Username, proxy.Password)
		};

		#endregion

		#region Explicit

		public static Proxy FromString(string str, ProxyType type) {
			var proxy = (Proxy)str;

			proxy.Type = type;

			return proxy;
		}
		
		public static explicit operator Proxy(string str) {
			var data = str.Split('@')
						  .SelectMany(x => x.Split(':'))
						  .ToArray();

			if (data.Length != 4) {
				throw new ArgumentNullException($"Proxy has no credentials or empty: {str}", nameof(str));
			}

			int host_index = -1;

			for (int i = 0; i < data.Length; i++) {
				if (data[i].Split('.').Length == 4) {
					host_index = i;
					break;
				}
			}

			var username_index = host_index switch {
				0 => 2,
				2 => 0,
				_ => throw new ArgumentException("Proxy's host not found")
			};

			return new() {
				Host = data[host_index],
				Port = uint.Parse(data[host_index + 1]),
				Username = data[username_index],
				Password = data[username_index + 1],
				Type = ProxyType.Http
			};
		}

		#endregion

		public async ValueTask DisposeAsync() {
			// Set properties
			IsInUse = false;
			
			// Save
			await Context.SaveChangesAsync();
		}
	}

	public enum ProxyType {
		Http,
		Https,
		Socks4,
		Socks5
	}
}