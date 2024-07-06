using System.Collections;

using Bot.Core.Messages.WTelegram;
using Bot.Core.Models.Commands.Abstractions;
using Bot.Telegram.Commands;

using SlimMessageBus;

using TL;

using WTelegram;

namespace Bot.Telegram.WTelegram {
	public class Bootstrapper(Client client, IConfiguration config, IMessageBus bus, ILogger<Bootstrapper> logger) : IHostedService {
		private UpdateManager _manager;

		public async Task StartAsync(CancellationToken cancellationToken) {
			await client.LoginBotIfNeeded(config["Bot:Token"]);
			_manager = client.WithUpdateManager(
				OnUpdate,
				".state",
				reentrant: true
			);

			await client.Bots_SetBotCommands(
				new BotCommandScopeUsers(),
				"en",
				Keys.GenerateCommands()
			);
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.Run(() => _manager.SaveState(".state"), cancellationToken);
		}

		private async Task OnUpdate(Update update) {
			var response = await bus.Send(
				new UpdateHandlerRequest {
					Update = update,
					UpdateManager = _manager
				},
				update.GetType().Name
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

			foreach (var command in commands) {
				try {
					await command.ExecuteAsync((TUpdate)command_update, user);
				}
				catch (Exception e) {
					logger.LogError(e, "Error was throwing while execution command");
				}
			}
		}
	}
}