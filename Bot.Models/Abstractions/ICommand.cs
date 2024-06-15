using TL;

namespace Bot.Models.Abstractions {
	public interface ICommand<in TUpdate> where TUpdate : Update {
		Task ExecuteAsync(TUpdate update, User user);
	}
}