using Bot.Core.Models.Parsers.Abstractions;

namespace Bot.Core.Models.Parsers.Base {
	public abstract class BaseParserStream<T> : IParserStream<T> where T : class {
		public ICollection<T> FromLogs(string logs, int max_threads) {
			var list = new List<T>();
			
			Directory.GetDirectories(logs).WithThreads(
				max_threads,
				(_, _) => Task.FromResult(true),
				on_success: async (log, _) => {
					list.AddRange(await FromLog(log).ToArrayAsync());
				}
			);

			return Distinct(list);
		}

		public abstract IAsyncEnumerable<T> FromLog(string log);
		
		// Protected
		protected abstract ICollection<T> Distinct(ICollection<T> collection);
	}
}