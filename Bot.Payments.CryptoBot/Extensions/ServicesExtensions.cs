using CryptoPay;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Payments.CryptoBot.Extensions {
	public static class ServicesExtensions {
		public static void AddBotPaymentsCryptoBotDependencies(this IServiceCollection services) {
			services.AddHttpClient();
			
			services.AddSingleton<ICryptoPayClient>(
				x => {
					var config = x.GetRequiredService<IConfiguration>();
					var http = x.GetRequiredService<HttpClient>();

					return new CryptoPayClient(
						config["CryptoPay:Token"],
						http,
						config["CryptoPay:ApiUrl"]
					);
				}
			);
		}
	}
}