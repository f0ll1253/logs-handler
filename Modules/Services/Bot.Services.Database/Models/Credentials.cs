using System.Text.RegularExpressions;

namespace Bot.Services.Database.Models {
	public class Credentials {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Protocol { get; set; }
		public string? DomainPath { get; set; }
		public string Domain { get; set; }
		public int? Port { get; set; }
		public string? Path { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		private static readonly Regex _url = new(@"([a-z]+):\/\/(.+\.)?([^\.]+)\.([a-z]+)(:[0-9]+)?(\/)(.+)?:(.+):(.+)");
		private static readonly Regex _ip = new(@"([a-z]+):\/\/((?:(?:\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])(?:\.(?!\:)|)){4})(\:(?!0)(\d{1,4}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?(\/)(.+)?:(.+):(.+)");
		public static implicit operator Credentials?(string str) {
			if (_url.IsMatch(str)) {
				return _ParseFromUrl(str);
			}
			
			if (_ip.IsMatch(str)) {
				return Credentials._ParseFromIp(str);
			}

			return null;
		}

		private static Credentials _ParseFromUrl(string str) {
			var match = _url.Match(str);
			int? port = null;

			if (!string.IsNullOrEmpty(match.Groups[5].Value)) {
				port = int.Parse(match.Groups[5].Value[1..]);
			}

			return new() {
				Protocol = match.Groups[1].Value,
				DomainPath = match.Groups[2].Value,
				Domain = $"{match.Groups[3].Value}.{match.Groups[4].Value}",
				Port = port,
				Path = match.Groups[7].Value,
				Username = match.Groups[8].Value,
				Password = match.Groups[9].Value
			};
		}

		private static Credentials? _ParseFromIp(string str) {
			var match = _ip.Match(str);
			int? port = null;

			// check local
			if (match.Groups[2].Value.StartsWith("192.168")) {
				return null;
			}
			
			if (!string.IsNullOrEmpty(match.Groups[4].Value)) {
				port = int.Parse(match.Groups[4].Value[1..]);
			}

			return new() {
				Protocol = match.Groups[1].Value,
				Domain = match.Groups[2].Value,
				Port = port,
				Path = match.Groups[6].Value,
				Username = match.Groups[7].Value,
				Password = match.Groups[8].Value
			};
		}
	}
}