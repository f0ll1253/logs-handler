using Bot.Core.Models.Abstractions;
using Bot.Core.Models.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Core.Models.Base {
	public abstract class BaseReadOnlyRepository<T, TKey>(DbContext context, ILogger? logger) : IReadOnlyRepository<T, TKey> where T : class {
		public virtual Task<bool> AddAsync(T entity) => _TrySaveAsync(context.AddAsync(entity).AsTask());

		public virtual Task<T?> GetAsync(TKey key) => context.FindAsync<T>(key).AsTask();
		
		// Protected
		protected Task<bool> _TrySaveAsync(Action action) => _TrySaveAsync(Task.Run(action));
		
		protected async Task<bool> _TrySaveAsync(Task? action = null) {
			if (action is not null) {
				await action;
			}
			
			try {
				await context.SaveChangesAsync();
			} catch (Exception e) {
				logger?.LogError(new ContextSaveException(e, "Proxies"), null);

				context.ChangeTracker.Clear();
                
				return false;
			}

			return true;
		}
	}
}