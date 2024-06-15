using Bot.Models.Payments;

using Microsoft.EntityFrameworkCore;

namespace Bot.Database {
	public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options) {
		public DbSet<Payment> Payments { get; set; }

		protected override void OnModelCreating(ModelBuilder builder) {
			builder.Entity<Payment>(
				entity => {
					entity.HasKey(x => x.Id);
				}
			);
			
			base.OnModelCreating(builder);
		}
	}
}