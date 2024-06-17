using System.Collections;

using Bot.Core.Messages.WTelegram;
using Bot.Core.Models.Commands.Abstractions;

using SlimMessageBus;

using TL;

using WTelegram;

namespace Bot.Telegram.WTelegram {
	public class Bootstrapper(Client client, IConfiguration config, IMessageBus bus) : IHostedService {
		private UpdateManager _manager;
		
		public async Task StartAsync(CancellationToken cancellationToken) {
			await client.LoginBotIfNeeded(config["Bot:Token"]);
			_manager = client.WithUpdateManager(
				OnUpdate,
				".state",
				reentrant: true
			);
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.CompletedTask; // ignore
		}

		private async Task OnUpdate(Update update) {
			var response = await bus.Send(
				new UpdateHandlerRequest {
					Update = update,
					UpdateManager = _manager
				}
			);

			if (response.User is null) {
				return;
			}

			await (Task)GetType()
						.GetMethod(nameof(Bootstrapper.ExecuteCommandsAsync))!
						.MakeGenericMethod(update.GetType())
						.Invoke(this, [response.Commands, update, response.User])!;
		}
		
		// public for reflection execution
		public async Task ExecuteCommandsAsync<TUpdate>(ICollection collection, Update command_update, User user) where TUpdate : Update {
			var commands = collection as ICollection<ICommand<TUpdate>>;
			
			foreach (ICommand<TUpdate> command in commands) {
				await command.ExecuteAsync((TUpdate)command_update, user);
			}
		}
	}
}