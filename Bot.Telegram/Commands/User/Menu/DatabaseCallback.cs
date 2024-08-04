using Bot.Core.Models.Commands.Abstractions;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Menu {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Menu.DatabaseCallback)]
	public class DatabaseCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			return client.Messages_EditMessage(
				user,
				update.msg_id,
				"Database",
				reply_markup: new ReplyInlineMarkup {
					rows = [
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Upload",
									data = [Keys.Database.UploadCallback]
								}
							]
						},
						new() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Select By Domain",
									data = [Keys.Database.SelectByDomainCallback]
								},
								new KeyboardButtonCallback {
									text = "Select By Username",
									data = [Keys.Database.SelectByUsernameCallback]
								},
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