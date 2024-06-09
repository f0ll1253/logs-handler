namespace Bot.Parsers.Abstractions {
	public interface IFileParser<out TOut> {
		IEnumerable<IAsyncEnumerable<TOut>> FromLogs(string logs);
		IEnumerable<IAsyncEnumerable<TOut>> FromLog(string log);
		IAsyncEnumerable<TOut> FromFile(string filepath);
	}
	
	public interface IFileParser<in TInput, out TOut> {
		IEnumerable<IAsyncEnumerable<TOut>> FromLogs(string logs, TInput input);
		IEnumerable<IAsyncEnumerable<TOut>> FromLog(string log, TInput input);
		IAsyncEnumerable<TOut> FromFile(string filepath, TInput input);
	}
}