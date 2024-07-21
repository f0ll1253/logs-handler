namespace Bot.Core.Models.Abstractions {
	public interface IReadOnlyRepository<T, in TKey> where T : class {
		Task<bool> AddAsync(T entity);
		Task<T?> GetAsync(TKey key);
	}
}