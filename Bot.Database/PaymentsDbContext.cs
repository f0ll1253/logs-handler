using Bot.Models.Payments;

using Microsoft.EntityFrameworkCore;

namespace Bot.Database {
	public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options) {
		public DbSet<Payment> Payments { get; set; }
		public DbSet<CryptoPayPaymentData> CryptoPayPaymentDatas { get; set; }

		protected override void OnModelCreating(ModelBuilder builder) {
			builder.Entity<Payment>(
				entity => {
					entity.HasKey(x => x.Id);

					entity.HasOne<CryptoPayPaymentData>(x => (CryptoPayPaymentData)x.Data)
						  .WithOne(x => x.Payment)
						  .HasPrincipalKey<Payment>(x => x.Id)
						  .HasForeignKey<CryptoPayPaymentData>(x => x.PaymentId);
				}
			);

			builder.Entity<CryptoPayPaymentData>(
				entity => {
					entity.HasKey(x => x.Id);
				}
			);
			
			base.OnModelCreating(builder);
		}
	}
}