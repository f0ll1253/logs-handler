using Bot.Database;
using Bot.Models.Proxies;
using Bot.Services.Abstractions;

using Injectio.Attributes;

using Microsoft.EntityFrameworkCore;

namespace Bot.Services {
	[RegisterScoped<IProxyRepository>]
	public class ProxiesRepository(DataDbContext context) : IProxyRepository {
		public async Task<Proxy> GetAsync() {
			var proxy = await context.Set<Proxy>().OrderBy(x => x.Index).FirstAsync();

			proxy.IsInUse = true;

			await context.SaveChangesAsync();
            
			return proxy;
		}

		public async Task CloseAsync(string id) {
			var proxy = await context.FindAsync<Proxy>(id);

			if (proxy is not { }) {
				throw new ArgumentException($"Proxy with id {id} not found", nameof(id));
			}
			
			proxy.IsInUse = false;

			await context.SaveChangesAsync();
		}
	}
}