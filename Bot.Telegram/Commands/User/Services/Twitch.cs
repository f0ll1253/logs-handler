using Bot.Core.Models;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Telegram.Commands.Common;
using Bot.Telegram.Services;

using Hangfire;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.TwitchCallback)]
	public class Twitch(
		//
		Client client,
		FilesManager files,
		IParserStream<Bot.Services.Twitch.Models.User> parser,
		
		//
		IConfiguration config, 
		ILogger<Twitch> logger) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var data = config.GetPath(update.data, Paths.Extracted);
			
			BackgroundJob.Enqueue(() => InvokeAsync(update.msg_id, data.name, data.path, user));

			return client.Messages_EditMessage(
				user,
				update.msg_id,
				$"Start processing: {data.name}",
				reply_markup: new ReplyInlineMarkup {
                    rows = [
						new() {
							buttons = [
								Buttons.Dispose_Button
							]
						}
					]
				}
			);
		}
		
		// Hangfire
		public async Task InvokeAsync(int msg_id, string name, string path, TL.User user) {
			var (_, telegram) = await files.CreateOrGetAsync(
				async stream => {
					logger.LogInformation("[Twitch] Parsing tokens");
					var tokens = parser.FromLogs(path, int.Parse(config["Multithreading:Parser"]));
			
					logger.LogInformation("[Twitch] Parsing completed");
					
					await using (var writer = new StreamWriter(stream, leaveOpen: true)) {
						foreach (var token in tokens) {
							await writer.WriteLineAsync(token.Token);
						}
					}
				},
				new(name, "txt", "Twitch"),
				new(name, "txt", "Twitch", "text/plain")
			);

			var text = "#twitch";
			
			await client.Messages_SendMedia(
				user,
				telegram,
				text,
				Random.Shared.NextInt64(),
				entities: client.MarkdownToEntities(ref text)
			);
			
			await new DisposeCallback(client).ExecuteAsync(new() {msg_id = msg_id}, user);
		}
	}
}