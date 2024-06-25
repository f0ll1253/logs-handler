using Bot.Core.Models.Payments.Abstractions;

namespace Bot.Core.Models.Abstractions {
	public interface IRepository<in T, in TKey> where T : class {
		Task<bool> AddAsync(T obj);
		Task<IPayment?> GetAsync(TKey key);
	}
}