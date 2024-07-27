using Bot.Services.Discord.Models;

using Leaf.xNet;

using Newtonsoft.Json;

namespace Bot.Services.Discord.Internal {
	internal class DiscordApi {

		public Task<bool> TryLogin(string token, out User user) {
			user = new();
			string content;
			
			// Make request
			using (var request = new HttpRequest()) { // TODO add proxy
				request.Authorization = token;

				var response = request.Get(Endpoints.User.Me);

				if (!response.IsOK) {
					return Task.FromResult(false);
				}

				content = response.ToString();
			}
			
			// Populate account
			JsonConvert.PopulateObject(content, user);

			return Task.FromResult(true);
		}
        
		private static class Endpoints {
			private const string Base = "https://discord.com/api/v9";
			
			public static class User {
				public const string Me = Base + "/users/@me";
				public const string Guilds = Base + "/users/@me/guilds";
				
				public static class Billing {
					public const string CountryCode = Base + "/users/@me/@me/billing/country-code";
					public const string PaymentSources = Base + "/users/@me/@me/billing/payment-sources";
				}
			}
		}
	}
}