using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.User.Callbacks.Cookies {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_cookies")]
	public class CookiesCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			return client.Messages_EditMessage(
				user,
				update.msg_id,
				"Cookies",
				reply_markup: Markup_User.CookiesMarkup(update.data.Utf8().Split(':')[1])
			);
		}
	}
}