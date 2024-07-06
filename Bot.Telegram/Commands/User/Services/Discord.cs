using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = Keys.Services.Discord), RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(Client client) : BaseView(client) {
		protected override Task<string> DefaultMessage(object args, TL.User user) {
			return base.DefaultMessage(args, user);
		}

		protected override Task<ReplyInlineMarkup?> DefaultMarkup(object args, TL.User user) {
			return base.DefaultMarkup(args, user);
		}
	}
}