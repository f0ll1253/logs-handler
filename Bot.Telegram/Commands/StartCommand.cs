using Bot.Core.Models.Commands.Abstractions;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands {
	[RegisterTransient(ServiceKey = "/start")]
	public class StartCommand(Client client) : ICommand<UpdateNewMessage> {
		public Task ExecuteAsync(UpdateNewMessage update, User user) {
			return client.Messages_SendMessage(
				user,
				$"Hello, {user.username}!",
				Random.Shared.NextInt64()
			);
		}
	}
}