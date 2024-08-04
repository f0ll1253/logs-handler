using Bot.Core.Models;
using Bot.Core.Models.Parsers.Base;
using Bot.Services.Discord.Models;

namespace Bot.Services.Discord {
	public class Parser : BaseParserStream<User> {
		public override async IAsyncEnumerable<User> FromLog(string log) {
			if (log.TryGetFilesOf("Discord") is not {Length: > 0} files) {
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

		protected override ICollection<User> Distinct(ICollection<User> collection) {
			return collection.DistinctBy(x => x.Token).ToList();
		}
	}
}