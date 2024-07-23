namespace Bot.Core.Models.Parsers.Abstractions {
	public interface IParserStream<T> where T : class {
		IAsyncEnumerable<T> FromLogs(string logs);
		IAsyncEnumerable<T> FromLog(string log);
	}
}