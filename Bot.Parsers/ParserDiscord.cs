using Bot.Parsers.Abstractions;

namespace Bot.Parsers {
	[RegisterScoped<IFileParser<string>>(ServiceKey = "parser_discord")]
	public class ParserDiscord() : BaseParser<string>("Discord") {
		public override IEnumerable<IAsyncEnumerable<string>> FromLogs(string logs) {
			return base.FromLogs(logs)
					   .Select(x => x.Distinct());
		}

		public override async IAsyncEnumerable<string> FromFile(string filepath) {
			using (var reader = new StreamReader(filepath)) {
				while (!reader.EndOfStream) {
					var line = await reader.ReadLineAsync();

					if (string.IsNullOrEmpty(line)) {
						continue;
					}
					
					yield return line;
				}
			}
		}
	}
}