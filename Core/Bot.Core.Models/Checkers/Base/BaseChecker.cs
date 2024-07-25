using System.Net;

using Bot.Core.Models.Checkers.Abstractions;

namespace Bot.Core.Models.Checkers.Base {
	public abstract class BaseChecker<T, TAuthData> : IChecker<T> where T : class {
		public abstract Task<bool> CheckAsync(T account, HttpClient http);
		public abstract Task<bool> DetailsAsync(T account, HttpClient http);

		protected abstract Task<HttpResponseMessage?> _SendRequestAsync(HttpMethod method, string url, TAuthData auth_data, HttpClient http);
	}
}