namespace Bot.Checkers.Abstractions {
	public interface ICookieValidator {
		Task<bool> IsValidCookie(IAsyncEnumerable<string> lines);
	}
}