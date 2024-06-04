namespace Bot.Checkers.Abstractions {
	public interface ICookieChecker : ICookieValidator {
		Task<bool> CheckCookie(IEnumerable<string> lines);
	}
}