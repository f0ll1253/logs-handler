using Bot.Commands.Base;
using Bot.Models.Abstractions;
using Bot.Models.Data;
using Bot.Parsers.Abstractions;
using Bot.Services;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace Bot.Commands.User.Callbacks {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_instagram")]
	public class InstagramCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : ProcessCallback(
		"Cookies", 
		"Instagram", 
		"zip", 
		"Parse Instagram cookies", 
		"Instagram cookies not found", 
		"#instagram #cookies", 
		
		client, tasks, files) {
		private protected override async Task<FileEntity?> _ParseAsync(string logs_name) {
			using var archive = ArchiveFactory.Create(ArchiveType.Zip);

			foreach (var cookies in parser.FromLogs(Path.Combine(Constants.Directory_Extracted, logs_name), [".instagram.com"])) {
				if (!await cookies.AnyAsync()) {
					continue;
				}

				var memory = new MemoryStream();

				await using (var writer = new StreamWriter(memory, leaveOpen: true)) {
					await foreach (var line in cookies) {
						await writer.WriteLineAsync(line);
					}
				}

				archive.AddEntry($"{Guid.NewGuid().ToString()}.txt", memory, true);
			}

			if (!archive.Entries.Any()) {
				return null;
			}

			return await files.SaveFileAsync(
				stream => archive.SaveTo(stream, new ZipWriterOptions(CompressionType.None)),
				Category,
				Service,
				logs_name,
				FileType
			);
		}
	}
}