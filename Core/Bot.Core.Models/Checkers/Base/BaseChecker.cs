using System.Net;

using Bot.Core.Models.Checkers.Abstractions;

namespace Bot.Core.Models.Checkers.Base {
	public abstract class BaseChecker<T, TAuthData> : IChecker<T> where T : class {
		public abstract Task<bool> CheckAsync(T account, WebProxy proxy);
		public abstract Task<bool> DetailsAsync(T account, WebProxy proxy);

		protected abstract Task<HttpResponseMessage?> _SendRequestAsync(HttpMethod method, string url, TAuthData auth_data, WebProxy proxy, HttpClient? http = null);
		
		protected static HttpClient _CreateClient(WebProxy proxy) {
			var handler = new HttpClientHandler {
				UseProxy = true,
				Proxy = proxy,
			};

			return new(handler, true);
		}
	}
}