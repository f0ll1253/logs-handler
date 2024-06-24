using Bot.Core.Messages.WTelegram;
using Bot.Core.Models.Commands.Abstractions;

using SlimMessageBus;

using TL;

namespace Bot.Telegram.WTelegram.UpdateHandlers.Base {
	public abstract class BaseUpdateHandler<TRequest, TResponse, TUpdate, TKey>(IServiceProvider provider) : IRequestHandler<TRequest, TResponse>
			where TRequest : UpdateHandlerRequest
			where TResponse : UpdateHandlerResponse, new()
			where TUpdate : Update {
		public virtual async Task<TResponse> OnHandle(TRequest request) {
			if (await ValidateRequestAsync(request) is not { } update) {
				return new();
			}

			var user = request.UpdateManager.Users[await GetUserIdAsync(update)];

			return new() {
				Commands = await GetCommands(await GetKeyAsync(update), user).ToListAsync(),
				User = user
			};
		}

		protected virtual Task<TUpdate?> ValidateRequestAsync(TRequest request) {
			if (request.Update is not TUpdate update) {
				return Task.FromResult<TUpdate?>(null);
			}

			return Task.FromResult<TUpdate?>(update);
		}

		protected abstract Task<TKey> GetKeyAsync(TUpdate update);
		protected abstract Task<long> GetUserIdAsync(TUpdate update);

		private protected async IAsyncEnumerable<ICommand<TUpdate>> GetCommands(TKey key, User user) {
			var commands = provider.GetKeyedServices<ICommand<TUpdate>>(key);
			var with_filters = false;

			foreach (var filter in commands.OfType<IFilter<User>>()) {
				if (await filter.CanExecute(user)) {
					with_filters = true;
					yield return (ICommand<TUpdate>)filter;
				}
			}

			if (with_filters) {
				yield break;
			}

			foreach (var command in commands) {
				yield return command;
			}
		}
	}
}