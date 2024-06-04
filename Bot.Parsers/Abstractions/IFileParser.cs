namespace Bot.Parsers.Abstractions {
	public interface IFileParser<in TInput, out TOut> {
		IEnumerable<IAsyncEnumerable<string>> FromLogs(string logs, TInput input);
		IEnumerable<IAsyncEnumerable<string>> FromLog(string log, TInput input);
		IAsyncEnumerable<TOut> FromFile(string filepath, TInput input);
	}
}