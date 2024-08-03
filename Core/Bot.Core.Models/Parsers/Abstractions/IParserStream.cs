namespace Bot.Core.Models.Parsers.Abstractions {
	public interface IParserStream<T> where T : class {
		ICollection<T> FromLogs(string logs, int max_threads);
		IAsyncEnumerable<T> FromLog(string log);
	}
}