using System.Diagnostics;
using System.Net;

using Bot.Models.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Bot.Models.Proxies {
	[DebuggerDisplay("{Host}:{Port}@{Username}:{Password}")]
	public class Proxy : IEntity<string> {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public int Index { get; set; }
		
		public string Host { get; set; }
		public int Port { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }

		public bool IsInUse { get; set; }

		public static implicit operator WebProxy(Proxy proxy) {
			var web = new WebProxy {
				Address = new($"http://{proxy.Host}:{proxy.Port}")
			};

			if (proxy is {Username: not null, Password: not null}) {
				web.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
			}

			return web;
		}
		
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
}