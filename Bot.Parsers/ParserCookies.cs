using Bot.Parsers.Abstractions;

namespace Bot.Parsers {
	[RegisterScoped<IFileParser<IEnumerable<string>, string>>(ServiceKey = "parser_cookies")]
	public class ParserCookies() : BaseInputParser<IEnumerable<string>, string>("Cookies") {
		public override async IAsyncEnumerable<string> FromFile(string filepath, IEnumerable<string> domains) {
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