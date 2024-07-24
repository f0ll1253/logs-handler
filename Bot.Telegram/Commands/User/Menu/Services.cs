using Bot.Core.Models.Commands.Abstractions;
using Bot.Telegram.Commands.Common;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Menu {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Menu.ServicesCallback)]
	public class Services(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			return client.Messages_EditMessage(
				user,
				update.msg_id,
				"Services",
				reply_markup: new ReplyInlineMarkup() {
					rows = [
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Discord",
									data = ShowLogsCallback.CreateData(Keys.Services.DiscordCallback, Keys.Menu.ServicesCallback)
								},
								new KeyboardButtonCallback {
									text = "Twitch",
									data = ShowLogsCallback.CreateData(Keys.Services.TwitchCallback, Keys.Menu.ServicesCallback)
								}
							]
						},
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Back",
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