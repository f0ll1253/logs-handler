using Bot.Core.Models.Parsers.Abstractions;
using Bot.Parsers.Discord.Models;

namespace Bot.Parsers.Discord {
	public class DiscordParser : IParserStream<Account> {
		public async IAsyncEnumerable<Account> FromLog(string log) {
			var folder = Path.Combine(log, "Discord");
			
			if (!Directory.Exists(folder) || Directory.GetFiles(folder) is not {Length:>0} files) {
				yield break;
			}

			foreach (var file in files) {
				using (var reader = new StreamReader(file)) {
					while (!reader.EndOfStream) {
						var line = await reader.ReadLineAsync();

						if (string.IsNullOrEmpty(line)) {
							continue;
						}

						yield return new() {
							Token = line.Trim()
						};
					}
				}
			}
		}
	}
}