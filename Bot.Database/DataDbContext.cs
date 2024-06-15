using Bot.Models.Files;
using Bot.Models.Proxies;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bot.Database {
	public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options) {
		// Files
		public DbSet<FileEntity> Files { get; set; }
		public DbSet<FileTelegramInfo> TelegramInfos { get; set; }
		
		// Proxies
		public DbSet<Proxy> Proxies { get; set; }

		protected override void OnModelCreating(ModelBuilder builder) {
			// Files
			builder.Entity<FileEntity>(
				entity => {
					entity.HasKey(x => x.Id);
					
					entity.HasOne(x => x.TelegramInfo)
						  .WithOne(x => x.File)
						  .HasPrincipalKey<FileEntity>(x => x.Id)
						  .HasForeignKey<FileTelegramInfo>(x => x.FileId);
				}
			);

			builder.Entity<FileTelegramInfo>(
				entity => {
					entity.HasKey(x => x.Id);
					
					entity.HasOne(x => x.File)
						  .WithOne(x => x.TelegramInfo)
						  .HasPrincipalKey<FileTelegramInfo>(x => x.Id)
						  .HasForeignKey<FileEntity>(x => x.TelegramInfoId);
				}
			);
			
			// Proxies
			builder.Entity<Proxy>(
				entity => {
					entity.HasKey(x => x.Id);

					entity.HasIndex(x => x.Index)
						  .IsDescending()
						  .IsUnique();
					
					entity.Property(x => x.Index)
						  .HasValueGenerator<Proxy.ProxyIndexGenerator>()
						  .ValueGeneratedOnAddOrUpdate();
				}
			);

			base.OnModelCreating(builder);
		}
		
		#region FIX ValueGenerator not working on update 
		#pragma warning disable ALL
		public override int SaveChanges(bool acceptAllChangesOnSuccess) {
			GenerateOnUpdate();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
			GenerateOnUpdate();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void GenerateOnUpdate() {
			foreach (EntityEntry entityEntry in ChangeTracker.Entries()) {
				if (entityEntry.State != EntityState.Modified) {
					continue;
				}
				
				foreach (PropertyEntry propertyEntry in entityEntry.Properties) {
					IProperty property = propertyEntry.Metadata;

					var valueGeneratorFactory = property.GetValueGeneratorFactory();
					var generatedOnUpdate = (property.ValueGenerated & ValueGenerated.OnUpdate) == ValueGenerated.OnUpdate;

					if (!generatedOnUpdate || valueGeneratorFactory == null) {
						continue;
					}

					var valueGenerator = valueGeneratorFactory.Invoke(
						property,
						entityEntry.Metadata
					);

					propertyEntry.CurrentValue = valueGenerator.Next(entityEntry);
				}
			}
		}
		#pragma warning restore
		#endregion
	}
}