namespace Bot.Core.Models.Parsers.Abstractions {
	public interface IParserStream<T> where T : class {
		IAsyncEnumerable<T> FromLog(string log);
	}
}