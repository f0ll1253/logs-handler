using System.Text;

using Bot.Parsers.Abstractions;

namespace Bot.Parsers {
	[RegisterScoped<IFileParser<string>>(ServiceKey = "parser_credentials")]
	public class ParserCredentials() : BaseParser<string>("Credentials") {
		public override IEnumerable<IAsyncEnumerable<string>> FromLog(string log) {
			return Directory.GetFiles(log, "*asswords.txt")
							.FirstOrDefault() is { } file ?
					new[] {
						FromFile(file)
					} :
					ArraySegment<IAsyncEnumerable<string>>.Empty;
		}

		public override async IAsyncEnumerable<string> FromFile(string filepath) {
			using (var reader = new StreamReader(filepath)) {
				var builder = new StringBuilder();
				
				while (!reader.EndOfStream) {
					var line = await reader.ReadLineAsync();

					// READLINE
					if (!line.StartsWith("URL")) {
						continue;
					}

					try {
						line = _GetValue(line);

						if (line[..line.IndexOf(':')] is not ("https" or "ftp")) {
							continue;
						}
						
						builder.Append(line);
						builder.Append(':' + _GetValue(await reader.ReadLineAsync()));
						builder.Append(':' + _GetValue(await reader.ReadLineAsync()));
					} catch {
						builder.Clear();
						
						continue;
					}

					yield return builder.ToString();

					builder.Clear();
				}
			}
		}

		private string _GetValue(string str) {
			var index = str.IndexOf(' ');

			if (index == -1) {
				throw new ArgumentException(nameof(str));
			}
			
			return str[(index + 1)..^1];
		}
	}
}