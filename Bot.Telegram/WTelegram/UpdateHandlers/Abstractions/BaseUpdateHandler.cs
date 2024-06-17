using Bot.Core.Models.Commands.Abstractions;

using SlimMessageBus;

using TL;

namespace Bot.Telegram.WTelegram.UpdateHandlers.Abstractions {
	public abstract class BaseUpdateHandler<TRequest, TResponse>(IServiceProvider provider) : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse> {
		public abstract Task<TResponse> OnHandle(TRequest request);
		
		private protected static string ParseKey(string str, char delimiter) {
			var index = str.IndexOf(delimiter);

			return str[..(index == -1 ? str.Length : index)];
		}

		private protected async IAsyncEnumerable<ICommand<TUpdate>> GetCommands<TUpdate>(string key, User user) where TUpdate : Update {
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