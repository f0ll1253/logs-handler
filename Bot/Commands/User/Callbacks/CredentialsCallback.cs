using Bot.Commands.Base;
using Bot.Models.Abstractions;
using Bot.Models.Data;
using Bot.Parsers.Abstractions;
using Bot.Services;

namespace Bot.Commands.User.Callbacks {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_credentials")]
	public class CredentialsCallback(ILogger<CredentialsCallback> logger, [FromKeyedServices("parser_credentials")] IFileParser<string> parser, Client client, TasksManager tasks, FilesManager files) : ProcessCallback(
		"Credentials", 
		"", 
		"txt", 
		"Parse credentials", 
		"Credentials not found", 
		"#credentials", 
		
		client, tasks, files) {
		private protected override async Task<FileEntity?> _ParseAsync(string logs_name) {
			var credentials = parser.FromLogs(Path.Combine(Directory_Extracted, logs_name))
									.ToAsyncEnumerable()
									.SelectMany(x => x)
									.Distinct();

			if (!await credentials.AnyAsync()) {
				return null;
			}
			
			await using (var memory = new MemoryStream()) {
				await using (var writer = new StreamWriter(memory, leaveOpen: true)) {
					await foreach (var row in credentials) {
						await writer.WriteLineAsync(row);

#if DEBUG
						logger.LogInformation(row);
#endif
					}
				}

				memory.Position = 0;

				return await files.SaveFileAsync(stream => memory.CopyTo(stream), Category, Service, logs_name, "txt");
			}
		}
	}
}