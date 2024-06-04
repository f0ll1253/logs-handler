using Bot.Bot.Abstractions;
using Bot.Models.Abstractions;

namespace Bot.Bot {
	[RegisterScoped<IUpdateHandler>]
	public class UpdateHandler(IServiceProvider provider, ILogger<IUpdateHandler> logger) : IUpdateHandler {
		public async Task HandleUpdateAsync(Update update, Dictionary<long, User> users) {
			ICollection<object> commands = [];
			User user = null;

			switch (update) {
				case UpdateNewMessage {message: Message {message: var text, peer_id: var id}}:
					if (text is null) {
						break;
					}

					user = users[id];
					commands = _FindCommands<UpdateNewMessage>(text, ' ') as ICollection<object>;
					break;
				case UpdateBotCallbackQuery {data: var data, user_id: var id}:
					user = users[id];
					commands = _FindCommands<UpdateBotCallbackQuery>(data.Utf8(), ':') as ICollection<object>;
					break;
				default:
					logger.LogWarning($"Command: unknown type '{update.GetType().Name}'");
					return;
			}

			await (Task)GetType()
						.GetMethod(nameof(UpdateHandler.ExecuteCommandsAsync))!
						.MakeGenericMethod(update.GetType())
						.Invoke(this, [commands, update, user])!;
		}

		public Task HandleErrorAsync(Update update, Dictionary<long, User> users, Exception exception) {
			logger.LogError(exception, "Error while handling update");
			
			return Task.CompletedTask;
		}
		
		// public for reflection execution
		public async Task ExecuteCommandsAsync<TUpdate>(ICollection<object> commands, Update command_update, User user) where TUpdate : Update {
			var filters = commands
						  .Where(x => x is IFilter<User>)
						  .Cast<IFilter<User>>()
						  .OrderBy(x => x.Order);

			foreach (var filter in filters) {
				if (!await filter.CanExecuteAsync(user)) {
					logger.LogWarning("User @{username} (#{id})", user.username, user.id);
					logger.LogWarning("Command: {type}", commands.GetType().Name);
					logger.LogWarning("State: can't execute");

					commands.Remove(filter);

					continue;
				}

				await ((ICommand<TUpdate>)filter).ExecuteAsync((TUpdate)command_update, user);

				return;
			}

			// if commands with filter was not executed, execute another commands
			foreach (ICommand<TUpdate> command in commands) {
				await command.ExecuteAsync((TUpdate)command_update, user);
			}
		}

		private ICollection<ICommand<TUpdate>> _FindCommands<TUpdate>(string text, char delimiter) where TUpdate : Update {
			var index = text.IndexOf(delimiter);
			var trigger = text[..(index == -1 ? text.Length : index)];
			var commands = provider.GetKeyedServices<ICommand<TUpdate>>(trigger).ToList();

			if (commands.Count == 0) { // TODO add state handler
				logger.LogWarning("Command: not found '{trigger}'", trigger);
			}

			return commands;
		}
	}
}