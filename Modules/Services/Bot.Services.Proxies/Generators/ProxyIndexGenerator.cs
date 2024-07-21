using Bot.Services.Proxies.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Bot.Services.Proxies.Generators {
	public class ProxyIndexGenerator : ValueGenerator<int> {
		public override bool GeneratesTemporaryValues { get; } = false;
		public override bool GeneratesStableValues { get; } = true;

		public override int Next(EntityEntry entry) {
			var entries = entry.Context.ChangeTracker.Entries<Proxy>();
			var set = entry.Context.Set<Proxy>();
			var max_entries = 0;
			var max_db = 0;

			if (entries.Any()) {
				max_entries = entries.Max(x => x.Entity.Index);
			}

			if (set.Any()) {
				max_db = set.Max(x => x.Index);
			}

			return Math.Max(max_entries, max_db) + 1;
		}

		public override async ValueTask<int> NextAsync(EntityEntry entry, CancellationToken cancellationToken = new CancellationToken()) {
			var entries = entry.Context.ChangeTracker.Entries<Proxy>();
			var set = entry.Context.Set<Proxy>();
			var max_entries = 0;
			var max_db = 0;

			if (entries.Any()) {
				max_entries = entries.Max(x => x.Entity.Index);
			}

			if (await set.AnyAsync(cancellationToken: cancellationToken)) {
				max_db = await set.MaxAsync(x => x.Index, cancellationToken: cancellationToken);
			}

			return Math.Max(max_entries, max_db) + 1;
		}
	}
}