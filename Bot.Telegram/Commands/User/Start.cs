using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User {
	[RegisterTransient(ServiceKey = Keys.Start, ServiceType = typeof(ICommand<UpdateNewMessage>)), RegisterTransient(ServiceKey = Keys.StartCallback, ServiceType = typeof(ICommand<UpdateBotCallbackQuery>))]
	public class Start(Client client) : BaseView(client) {
		public override Task<string> BuildMessage(UpdateNewMessage update, TL.User user) {
			return Task.FromResult($"Hello, {user.username}!");
		}

		public override Task<ReplyInlineMarkup?> BuildMarkup(UpdateNewMessage update, TL.User user) {
			return Task.FromResult<ReplyInlineMarkup?>(new() {
				rows = [
					new() {
						buttons = [
							new KeyboardButtonCallback() {
								text = "Games",
								data = [Keys.Menu.GamesCallback]
							},
							new KeyboardButtonCallback() {
								text = "Services",
								data = [Keys.Menu.ServicesCallback]
							},
						]
					}
				]
			});
		}
	}
}