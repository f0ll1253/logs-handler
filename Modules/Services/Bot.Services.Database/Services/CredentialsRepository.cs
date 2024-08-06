using System.Text;

using Bot.Core.Models.Base;
using Bot.Services.Database.Models;
using Bot.Services.Database.Models.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Thinktecture;
using Thinktecture.EntityFrameworkCore.BulkOperations;

namespace Bot.Services.Database.Services {
	public class CredentialsRepository(DbContext context, ILogger? logger) : BaseRepository<Credentials, string>(context, logger), ICredentialsRepository<Credentials> {
		public async Task AddRangeAsync(string filepath, IBulkInsertOptions? options = null) {
			if (options is not null) {
				await context.BulkInsertAsync(
					(await File.ReadAllLinesAsync(filepath, Encoding.ASCII)).Distinct().Select(x => (Credentials?)x).Where(x => x is not null),
					options
				);
			}
			else {
				await context.BulkInsertAsync(
					(await File.ReadAllLinesAsync(filepath, Encoding.ASCII)).Distinct().Select(x => (Credentials?)x).Where(x => x is not null)
				);
			}
		}

		public IAsyncEnumerable<Credentials> TakeBy(Func<Credentials, bool> prediction) => context.Set<Credentials>().Where(prediction).ToAsyncEnumerable();
	}
}