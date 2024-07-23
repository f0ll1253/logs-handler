using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace Bot.Tests {
	public static class Extensions {
		public static T GetConfiguration<T>(this string key) {
			var config = new ConfigurationBuilder()
						 .AddUserSecrets(Assembly.GetExecutingAssembly())
						 .AddInMemoryCollection()
						 .Build();

			return config.GetSection(key).Get<T>()!;
		}
	}
}