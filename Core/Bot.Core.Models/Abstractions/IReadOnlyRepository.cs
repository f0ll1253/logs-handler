namespace Bot.Core.Models.Abstractions {
	public interface IReadOnlyRepository<T, in TKey> where T : class {
		Task<bool> AddAsync(T entity);
		Task<bool> AddRangeAsync(ICollection<T> collection);
		Task<T?> GetAsync(TKey key);
	}
}