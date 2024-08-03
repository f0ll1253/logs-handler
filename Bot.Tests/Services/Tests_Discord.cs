using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Discord;
using Bot.Services.Discord.Models;
using Bot.Services.Proxies.Models;

using Newtonsoft.Json;

namespace Bot.Tests.Services {
	public class Tests_Discord {
		private readonly List<User> _accounts = new();
		
		private DiscordConfiguration _configuration;
		private IParserStream<User> _parser;
		private IChecker<User> _checker;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_configuration = "Discord".GetConfiguration<DiscordConfiguration>();

			_parser = new Parser();
			_checker = new Checker(null);
		}

		#region Parser

		[Test]
		public async Task Test_FromLogs() {
			foreach (var account in _parser.FromLogs(_configuration.Logs, 10)) {
				await TestContext.Out.WriteLineAsync(account.Token);
			}
		}
		
		[Test]
		public async Task Test_FromLog() {
			await foreach (var account in _parser.FromLog(_configuration.Log)) {
				await TestContext.Out.WriteLineAsync(account.Token);
			}
		}

		#endregion

		#region Checker

		[Test, Order(0)]
		public async Task Test_CheckValid() {
			foreach (User account in _configuration.Valid.ToArray()) {
				Assert.That(await _checker.CheckAsync(account, (Proxy)_configuration.Proxy), Is.True);
				
				_accounts.Add(account);

				await TestContext.Out.WriteAsync(JsonConvert.SerializeObject(account, Formatting.Indented));
			}
		}

		[Test]
		public async Task Test_CheckInvalid() {
			foreach (User account in _configuration.Invalid) {
				Assert.That(await _checker.CheckAsync(account, (Proxy)_configuration.Proxy), Is.False);
			}
		}

		[Test, Order(1)]
		public async Task Test_Details() {
			var proxy = (Proxy)_configuration.Proxy;
			
			foreach (var account in _accounts) {
				Assert.That(await _checker.DetailsAsync(account, proxy), Is.True);
				
				await TestContext.Out.WriteAsync(JsonConvert.SerializeObject(account, Formatting.Indented));
			}
		}

		#endregion
	}

	public class DiscordConfiguration {
		public string Logs { get; set; }
		public string Log { get; set; }
		
		public string Proxy { get; set; }
		public List<string> Valid { get; set; } = new();
		public List<string> Invalid { get; set; } = new();
	}
}