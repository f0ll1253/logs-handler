namespace Bot.Core.Models.Abstractions {
	public interface IRepository<T, in TKey> : IReadOnlyRepository<T, TKey> where T : class {
		Task<bool> AddRangeAsync(ICollection<T> collection);
		
		Task<bool> RemoveAsync(string key);
		
		Task<bool> UpdateAsync(T entity);
	}
}