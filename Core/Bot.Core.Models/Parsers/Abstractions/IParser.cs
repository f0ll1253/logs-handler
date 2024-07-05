namespace Bot.Core.Models.Parsers.Abstractions {
	public interface IParser<T> where T : class {
		Task<T?> FromLog(string log);
	}
}