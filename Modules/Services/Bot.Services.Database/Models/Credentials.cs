using System.Text.RegularExpressions;

using Bot.Services.Database.Models.Abstractions;

using Microsoft.VisualBasic;

namespace Bot.Services.Database.Models {
	public class Credentials : ICredentials {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Protocol { get; set; }
		public string? DomainPath { get; set; }
		public string Domain { get; set; }
		public int? Port { get; set; }
		public string? Path { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public static implicit operator Credentials?(string str) {
			var credentials = new Credentials();
			var data = str.Split(':');
			var index = 0;

			if (data.Length <= 3 || data[0].ToLower() == "unknown" || data[0] == "about") {
				return null;
			}

			// Protocol
			credentials.Protocol = data[index];

			index++;

			// Domain / DomainPath
			if (data[index].Length <= 2) {
				return null;
			}
			
			data[index] = data[index][2..];
			var slash_index = data[index].IndexOf('/');
			var full_domain = data[index];

			if (slash_index != -1) {
				full_domain = full_domain[..slash_index];
			}

			if (full_domain.StartsWith("192.168") || full_domain.StartsWith("localhost")) {
				return null;
			}

			var last_index = full_domain.LastIndexOf('.');

			if (last_index == -1) {
				credentials.Domain = full_domain;
			}
			else {
				var pre_last_index = full_domain[..last_index].LastIndexOf('.');

				credentials.Domain = full_domain[(pre_last_index + 1)..];

				if (pre_last_index > 0) {
					credentials.DomainPath = full_domain[..pre_last_index];
				}
			}

			// Port
			if (slash_index == -1) {
				index++;

				slash_index = data[index].IndexOf('/');

				if (slash_index != -1 && int.TryParse(data[index][..slash_index], out var port)) {
					credentials.Port = port;
				}
			}

			// Path
			credentials.Path = data[index][(slash_index + 1)..];

			index++;

			// Username / Password
			credentials.Username = string.Join(':', data.Take(new Range(index, data.Length - 1)));
			credentials.Password = data[^1];

			return credentials;
		}
	}
}