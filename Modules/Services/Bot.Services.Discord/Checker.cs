using System.Net;

using Bot.Core.Models.Checkers.Base;
using Bot.Services.Discord.Models;
using Bot.Services.Discord.Models.Guilds;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bot.Services.Discord {
	public class Checker(ILogger? logger) : BaseChecker<User, string> {
		private const string _Base_Url = "https://discord.com/api/v9/users/@me";
		
		public override async Task<bool> CheckAsync(User user, HttpClient http) {
			if (await _SendRequestAsync(HttpMethod.Get, _Base_Url, user.Token, http) is not { } response) {
				return false;
			}
			
			logger?.LogInformation("[Discord] Valid token: {token}", user.Token);
			JsonConvert.PopulateObject(await response.Content.ReadAsStringAsync(), user);

			return true;
		}
		
		public override async Task<bool> DetailsAsync(User user, HttpClient http) {
			return await GuildsAsync(user, http) && await PaymentSourcesAsync(user, http) && await ChannelsAsync(user, http) && await FriendsAsync(user, http);
		}
		
		public async Task<bool> GuildsAsync(User user, HttpClient? http = null) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/guilds?with_counts=true", user.Token, http) is not { } response ||
				JsonConvert.DeserializeObject<List<GuildDataClass>>(await response.Content.ReadAsStringAsync()) is not { } guilds) {
				return false;
			}

			user.Guilds = guilds;

			return true;
		}
		
		public async Task<bool> PaymentSourcesAsync(User user, HttpClient? http = null) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/billing/payment-sources", user.Token, http) is not { } response) {
				return false;
			}

			var content = await response.Content.ReadAsStringAsync();

			if (string.IsNullOrEmpty(content) || content == "[]\n") {
				return true;
			}

			throw new NotImplementedException();
		}

		public async Task<bool> ChannelsAsync(User user, HttpClient http) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/channels", user.Token, http) is not { } response ||
				JsonConvert.DeserializeObject<List<ChannelDataClass>>(await response.Content.ReadAsStringAsync()) is not { } channels) {
				return false;
			}

			user.Channels = channels;

			return true;
		}

		public async Task<bool> FriendsAsync(User user, HttpClient http) {
			if (await _SendRequestAsync(HttpMethod.Get, $"{_Base_Url}/relationships", user.Token, http) is not { } response ||
				JsonConvert.DeserializeObject<List<RealationShipDataClass>>(await response.Content.ReadAsStringAsync()) is not { } friends) {
				return false;
			}

			user.Friends = friends;

			return true;
		}
		
		//
		protected override async Task<HttpResponseMessage?> _SendRequestAsync(HttpMethod method, string url, string auth_data, HttpClient http) {
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

			var response = await http.SendAsync(request);
			
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