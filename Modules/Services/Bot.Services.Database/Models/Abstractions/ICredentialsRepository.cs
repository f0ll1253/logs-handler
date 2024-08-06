using Thinktecture.EntityFrameworkCore.BulkOperations;

namespace Bot.Services.Database.Models.Abstractions {
	public interface ICredentialsRepository<out TCredentials> where TCredentials : ICredentials {
		Task AddRangeAsync(string filepath, IBulkInsertOptions? options = null);
		IAsyncEnumerable<Credentials> TakeBy(Func<TCredentials, bool> prediction);
	}
}