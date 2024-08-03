using System.Text;

using Bot.Core.Models;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Services.Files.System.Services;
using Bot.Services.Proxies.Services;

using Injectio.Attributes;

using Newtonsoft.Json;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCheckCallback)]
	public class DiscordCheck(
		Client client,
		Proxies proxies,
		SystemFilesRepository files_system,
		IChecker<Bot.Services.Discord.Models.User> checker,
		
		IConfiguration config,
		ILogger<DiscordCheck> logger) : ICommand<UpdateBotCallbackQuery> {
		public async Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var id = Encoding.UTF8.GetString(update.data[1..]);

			await new Start(client).ExecuteAsync(update, user);
			
			logger.LogInformation("[Discord] Taking proxy");
			var proxy_list = await proxies.TakeAsync(int.Parse(config["Multithreading:Proxy"]!)).ToListAsync();

			logger.LogInformation("[Discord] Parse tokens from file {id}", id);
			if (await files_system.GetAsync(id) is not { } file) {
				logger.LogWarning("[Discord] File with id {id} not found", id);
				
				return;
			}

			var tokens = new List<Bot.Services.Discord.Models.User>();

			using (var reader = new StreamReader(File.OpenRead(Path.Combine(config["Files:Root"], file.Service, $"{file.Name}.{file.Extension}")))) {
				while (!reader.EndOfStream) {
					tokens.Add((await reader.ReadLineAsync()).Trim());
				}
			}
			
			tokens.WithThreads(
				int.Parse(config["Multithreading:Proxy"]!),
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

			var text = $"Checking of {file.Name}.{file.Extension} completed\n#discord";

			await client.Messages_SendMessage(
				user,
				text,
				Random.Shared.NextInt64(),
				entities: client.MarkdownToEntities(ref text),
				reply_markup: new ReplyInlineMarkup {
					rows = [
						new() {
							buttons = [
								Keys.Common.Dispose_Button
							]
						}
					]
				}
			);
		}
	}
}