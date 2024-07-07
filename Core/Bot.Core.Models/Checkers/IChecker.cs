namespace Bot.Core.Models.Checkers {
	public interface IChecker<T> where T : class {
		Task<bool> CheckAsync(T account);
	}
}