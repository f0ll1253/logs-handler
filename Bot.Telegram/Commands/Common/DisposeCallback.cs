using Bot.Core.Models.Commands.Abstractions;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.Common {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.DisposeCallback)]
	public class DisposeCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			return client.Messages_DeleteMessages([update.msg_id], true);
		}
	}
}