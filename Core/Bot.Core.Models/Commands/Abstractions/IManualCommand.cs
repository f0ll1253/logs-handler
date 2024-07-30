using WTelegram;

namespace Bot.Core.Models.Commands.Abstractions {
	public interface IManualCommand<in TArgs> where TArgs : class {
		Client Client { get; init; }

		Task ExecuteAsync(TL.User user, TArgs args);
	}
}