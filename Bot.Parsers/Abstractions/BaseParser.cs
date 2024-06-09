namespace Bot.Parsers.Abstractions {
	public abstract class BaseParser<TOut>(string subdirectory) : IFileParser<TOut> {
		public virtual IEnumerable<IAsyncEnumerable<TOut>> FromLogs(string logs) {
			return Directory
				   .GetDirectories(logs)
				   .SelectMany(FromLog);
		}

		public virtual IEnumerable<IAsyncEnumerable<TOut>> FromLog(string log) {
			return new DirectoryInfo(Path.Combine(log, subdirectory)) is {Exists: true} directory ?
					directory
							.GetFiles()
							.Select(x => FromFile(x.FullName)) :
					ArraySegment<IAsyncEnumerable<TOut>>.Empty;
		}

		public abstract IAsyncEnumerable<TOut> FromFile(string filepath);
	}

	public abstract class BaseInputParser<TInput, TOut>(string subdirectory) : IFileParser<TInput, TOut> {
		public virtual IEnumerable<IAsyncEnumerable<TOut>> FromLogs(string logs, TInput input) {
			return Directory
				   .GetDirectories(logs)
				   .SelectMany(x => FromLog(x, input));
		}

		public virtual IEnumerable<IAsyncEnumerable<TOut>> FromLog(string log, TInput input) {
			return new DirectoryInfo(Path.Combine(log, subdirectory)) is {Exists: true} directory ?
					directory
							.GetFiles()
							.Select(x => FromFile(x.FullName, input)) :
					ArraySegment<IAsyncEnumerable<TOut>>.Empty;
		}

		public abstract IAsyncEnumerable<TOut> FromFile(string filepath, TInput input);
	}
}