using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bot.Core.Models.Overrides {
	public abstract class DbContext : Microsoft.EntityFrameworkCore.DbContext {
		protected DbContext() { }
		
		protected DbContext(DbContextOptions options) : base(options) { }
		
		// FIX ValueGenerator not working on update 
		
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
	}
}