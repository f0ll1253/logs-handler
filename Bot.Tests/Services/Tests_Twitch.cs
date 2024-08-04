using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Twitch;
using Bot.Services.Twitch.Models;

namespace Bot.Tests.Services {
	public class Tests_Twitch {
		private TwitchConfiguration _configuration;
		
		private IParserStream<User> _parser;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_configuration = "Twitch".GetConfiguration<TwitchConfiguration>();
			
			_parser = new Parser();
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
	}
	
	public class TwitchConfiguration {
		public string Logs { get; set; }
		public string Log { get; set; }
	}
}