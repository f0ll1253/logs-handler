using WTelegram;

namespace Bot.Core.Models.Commands.Abstractions {
	public interface IManualCommand<in TArgs> where TArgs : class {
		Task ExecuteAsync(TL.User user, TArgs args);
	}
}