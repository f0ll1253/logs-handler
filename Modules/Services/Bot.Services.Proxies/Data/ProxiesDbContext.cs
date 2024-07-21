using Bot.Services.Proxies.Generators;
using Bot.Services.Proxies.Models;

using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Proxies.Data {
	public class ProxiesDbContext : Bot.Core.Models.Overrides.DbContext {
		public DbSet<Proxy> Proxies { get; set; }

		protected override void OnModelCreating(ModelBuilder builder) {
			builder.Entity<Proxy>(
				entity => {
					entity.HasKey(x => x.Id);

					entity.Property(x => x.Host)
						  .IsRequired();
					entity.Property(x => x.Port)
						  .IsRequired();
					entity.Property(x => x.Username)
						  .IsRequired();
					entity.Property(x => x.Password)
						  .IsRequired();

					entity.Ignore(x => x.Context);
					
					entity.HasIndex(x => x.Index)
						  .IsDescending()
						  .IsUnique();
					
					entity.Property(x => x.Index)
						  .HasValueGenerator<ProxyIndexGenerator>()
						  .ValueGeneratedOnAddOrUpdate();
				}
			);
			
			base.OnModelCreating(builder);
		}
	}
}