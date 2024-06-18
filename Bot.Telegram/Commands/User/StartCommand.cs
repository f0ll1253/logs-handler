using Bot.Core.Models.Commands.Abstractions;
using Bot.Core.Models.Commands.Base;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.User {
	[RegisterTransient(ServiceKey = Keys.Start, ServiceType = typeof(ICommand<UpdateNewMessage>)), RegisterTransient(ServiceKey = Keys.StartCallback, ServiceType = typeof(ICommand<UpdateBotCallbackQuery>))]
	public class StartCommand(Client client) : BaseView(client) {
		public override Task<string> BuildMessage(UpdateNewMessage update, TL.User user) => Task.FromResult($"Hello, {user.username}!");

		public override Task<ReplyInlineMarkup?> BuildMarkup(UpdateNewMessage update, TL.User user) => Task.FromResult<ReplyInlineMarkup?>(null);
	}
}