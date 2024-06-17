using TL;

namespace Bot.Core.Models.Commands.Abstractions {
	public interface ICommand<in TUpdate> where TUpdate : Update {
		Task ExecuteAsync(TUpdate update, User user);
	}
}