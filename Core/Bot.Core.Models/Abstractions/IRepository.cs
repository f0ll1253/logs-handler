namespace Bot.Core.Models.Abstractions {
	public interface IRepository<in T> where T : class {
		Task<bool> AddAsync(T obj);
	}
}