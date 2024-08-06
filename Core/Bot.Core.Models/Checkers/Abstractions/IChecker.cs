namespace Bot.Core.Models.Checkers.Abstractions {
	public interface IChecker<T> where T : class {
		Task<bool> CheckAsync(T account, HttpClient http);
		Task<bool> DetailsAsync(T account, HttpClient http);
	}
}