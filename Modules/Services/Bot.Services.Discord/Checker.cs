using System.Net;

using Bot.Core.Models.Checkers.Base;
using Bot.Services.Discord.Models;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bot.Services.Discord {
	public class Checker(ILogger? logger) : BaseChecker<Account, string> {
		private const string _Base_Url = "https://discord.com/api/v9/users/@me";
		
		public override async Task<bool> CheckAsync(Account account, WebProxy proxy) {
			if (await _SendRequestAsync(HttpMethod.Get, _Base_Url, account.Token, proxy) is not { } response) {
				return false;
			}
			
			logger?.LogInformation("[Discord] Valid token: {token}", account.Token);
			JsonConvert.PopulateObject(await response.Content.ReadAsStringAsync(), account);

			return true;
		}
		
		public override async Task<bool> DetailsAsync(Account account, WebProxy proxy) {
			using (var http = _CreateClient(proxy)) {
				return await GuildsAsync(account, http: http) && await PaymentSourcesAsync(account, http: http);
			}
		}
		
		// {{base_url}}/guilds
		public async Task<bool> GuildsAsync(Account account, WebProxy? proxy = null, HttpClient? http = null) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/guilds", account.Token, proxy, http) is not { } response ||
				JsonConvert.DeserializeObject<List<Account.GuildDataClass>>(await response.Content.ReadAsStringAsync()) is not { } guilds) {
				return false;
			}

			account.Guilds = guilds;

			return true;
		}
		
		// {{base_url}}/billing/payment-sources
		public async Task<bool> PaymentSourcesAsync(Account account, WebProxy? proxy = null, HttpClient? http = null) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/billing/payment-sources", account.Token, proxy, http) is not { } response) {
				return false;
			}

			var content = await response.Content.ReadAsStringAsync();

			if (string.IsNullOrEmpty(content) || content == "[]\n") {
				return true;
			}

			throw new NotImplementedException();
		}
		
		//
		protected override async Task<HttpResponseMessage?> _SendRequestAsync(HttpMethod method, string url, string auth_data, WebProxy? proxy = null, HttpClient? http = null) {
			if (proxy == null && http == null) {
				throw new ArgumentException("Required Proxy or HttpClient", $"{nameof(proxy)} or {nameof(http)}");
			}
			
			HttpRequestMessage request;

			try {
				request = new(method, url) {
					Headers = {
						Authorization = new(auth_data)
					}
				};
			} catch {
				return null;
			}

			HttpResponseMessage response;

			if (http == null) {
				using (http = _CreateClient(proxy)) {
					try {
						response = await http.SendAsync(request);
					} catch (TaskCanceledException) {
						return null;
					}
				}
			}
			else {
				response = await http.SendAsync(request);
			}
			
			if (!response.IsSuccessStatusCode) {
				switch (response.StatusCode) {
					case (HttpStatusCode)401:
						logger?.LogWarning("[Discord] [401] Token is invalid: {token}", auth_data);
						break;
					default:
						logger?.LogError("[Discord] [Unknown] Unknown error code: {code}\n{content}", (int)response.StatusCode, await response.Content.ReadAsStringAsync());
						break;
				}
					
				return null;
			}

			return response;
		}
	}
}