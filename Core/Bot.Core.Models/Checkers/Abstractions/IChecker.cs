using System.Net;

namespace Bot.Core.Models.Checkers.Abstractions {
	public interface IChecker<T> where T : class {
		Task<bool> CheckAsync(T account, WebProxy proxy);
		Task<bool> DetailsAsync(T account, WebProxy proxy);
	}
}