using System.Net;

using Bot.Core.Models.Proxies.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Proxies.Models {
	public class Proxy : IProxy, IEquatable<Proxy>, IAsyncDisposable {
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
		
		// From Proxy
		public static implicit operator string(Proxy proxy) => $"{proxy.Host}:{proxy.Port}:{proxy.Username}:{proxy.Password}";

		public static implicit operator WebProxy(Proxy proxy) => new() {
			Address = new($"{proxy.Type}://{proxy.Host}:{proxy.Port}"),
			Credentials = new NetworkCredential(proxy.Username, proxy.Password)
		};
		
		// To Proxy
		public static implicit operator Proxy(string str) {
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

		public async ValueTask DisposeAsync() {
			// Set properties
			IsInUse = false;
			
			// Save
			await Context.SaveChangesAsync();
		}

		public bool Equals(Proxy? other) {
			if (ReferenceEquals(null, other)) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			return Id == other.Id && Host == other.Host && Port == other.Port && Username == other.Username && Password == other.Password && Type == other.Type;
		}

		public override bool Equals(object? obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != this.GetType()) {
				return false;
			}
			return Equals((Proxy)obj);
		}

		public override int GetHashCode() {
			var hash_code = new HashCode();
			
			hash_code.Add(Id);
			hash_code.Add(Host);
			hash_code.Add(Port);
			hash_code.Add(Username);
			hash_code.Add(Password);
			hash_code.Add((int)Type);
			
			return hash_code.ToHashCode();
		}
	}

	public enum ProxyType {
		Http,
		Https,
		Socks4,
		Socks5
	}
}