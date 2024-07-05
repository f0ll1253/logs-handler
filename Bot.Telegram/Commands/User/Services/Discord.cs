using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User.Services {
	[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = Keys.Services.Discord), RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.Services.DiscordCallback)]
	public class Discord(Client client) : BaseView(client) {
		public override Task<string> BuildMessage(UpdateNewMessage update, TL.User user) {
			throw new NotImplementedException();
		}

		public override Task<ReplyInlineMarkup?> BuildMarkup(UpdateNewMessage update, TL.User user) {
			throw new NotImplementedException();
		}
	}
}