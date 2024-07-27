using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Discord.Models;

namespace Bot.Services.Discord {
	public class Parser : IParserStream<User> {
		public async IAsyncEnumerable<User> FromLogs(string logs) {
			if (!Directory.Exists(logs)) {
				yield break;
			}

			foreach (var log in Directory.GetDirectories(logs)) {
				await foreach (var account in FromLog(log)) {
					yield return account;
				}
			}
		}
		
		public async IAsyncEnumerable<User> FromLog(string log) {
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