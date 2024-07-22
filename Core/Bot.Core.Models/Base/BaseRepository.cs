using Bot.Core.Models.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Core.Models.Base {
	public abstract class BaseRepository<T, TKey>(DbContext context, ILogger? logger) : BaseReadOnlyRepository<T, TKey>(context, logger), IRepository<T, TKey> where T : class {
		public virtual Task<bool> AddRangeAsync(ICollection<T> collection) => _TrySaveAsync(context.AddRangeAsync(collection));

		public virtual async Task<bool> RemoveAsync(string key) {
			if (await context.FindAsync<T>(key) is not { } entity) {
				return false;
			}

			return await _TrySaveAsync(() => context.Remove(entity));
		}

		public virtual Task<bool> UpdateAsync(T entity) => _TrySaveAsync(() => context.Update(entity));
	}
}