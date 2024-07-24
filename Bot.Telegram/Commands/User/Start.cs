using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User {
	[RegisterTransient(ServiceKey = Keys.Start, ServiceType = typeof(ICommand<UpdateNewMessage>)), RegisterTransient(ServiceKey = Keys.StartCallback, ServiceType = typeof(ICommand<UpdateBotCallbackQuery>))]
	public class Start(Client client) : BaseView(client) {
		protected override Task<string> DefaultMessage(object args, TL.User user) {
			return Task.FromResult($"Hello, {user.username}!");
		}

		protected override Task<ReplyInlineMarkup?> DefaultMarkup(object args, TL.User user) {
			return Task.FromResult<ReplyInlineMarkup?>(new() {
				rows = [
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "Games",
								data = [Keys.Menu.GamesCallback]
							},
							new KeyboardButtonCallback {
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