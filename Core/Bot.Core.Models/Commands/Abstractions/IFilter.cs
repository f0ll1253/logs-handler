namespace Bot.Core.Models.Commands.Abstractions {
	public interface IFilter<T> where T : class {
		Task<bool> CanExecute(T obj);
	}
}