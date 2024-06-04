using Bot.Parsers.Abstractions;

namespace Bot.Parsers {
	[RegisterScoped<IFileParser<IEnumerable<string>, string>>]
	public class ParserCookies : IFileParser<IEnumerable<string>, string> {
		public IEnumerable<IAsyncEnumerable<string>> FromLogs(string logs, IEnumerable<string> input) {
			return Directory
				   .GetDirectories(logs)
				   .SelectMany(x => FromLog(x, input));
		}

		public IEnumerable<IAsyncEnumerable<string>> FromLog(string log, IEnumerable<string> input) {
			return new DirectoryInfo(Path.Combine(log, "Cookies")) is {Exists: true} directory ?
					directory
							.GetFiles()
							.Select(x => FromFile(x.FullName, input)) :
					ArraySegment<IAsyncEnumerable<string>>.Empty;
		}

		public async IAsyncEnumerable<string> FromFile(string filepath, IEnumerable<string> domains) {
			using (var reader = new StreamReader(filepath)) {
				while (!reader.EndOfStream) {
					var line = await reader.ReadLineAsync();
					var index = line.IndexOf('\t');

					if (index == -1) {
						continue;
					}

					var domain = line[..index];

					if (domains.Contains(domain)) {
						yield return line;
					}
				}
			}
		}
	}
}