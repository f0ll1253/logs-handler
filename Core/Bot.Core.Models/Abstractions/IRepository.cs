namespace Bot.Core.Models.Abstractions {
	public interface IRepository<T, in TKey> where T : class {
		Task<bool> AddAsync(T obj);
		Task<T?> GetAsync(TKey key);
	}
}