using Bot.Core.Models.Checkers;
using Bot.Services.Discord.Models;

namespace Bot.Services.Discord {
	public class Checker : IChecker<Account> {
		public Task<bool> CheckAsync(Account account) {
			throw new NotImplementedException();
		}
	}
}