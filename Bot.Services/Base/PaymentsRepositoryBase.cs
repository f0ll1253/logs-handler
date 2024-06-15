using Bot.Database;
using Bot.Models.Payments;
using Bot.Services.Abstractions;

namespace Bot.Services.Base {
	public abstract class PaymentsRepositoryBase<TInvoice>(PaymentsDbContext context) : IPaymentsRepository<TInvoice> where TInvoice : class {
		public abstract Task<TInvoice> CreateAsync(long user_id, string currency, double amount, string? description = null, string? return_callback = null);
		
		public virtual Task ExpireAsync(string id, DateTime date) => _UpdateStatusAsync(id, PaymentStatus.Expired, date);

		public virtual Task CompleteAsync(string id, DateTime date) => _UpdateStatusAsync(id, PaymentStatus.Paid, date);
		
		// CRUD
		public virtual ICollection<Payment> ActivePaymentsAsync(PaymentService service) {
			return context.Set<Payment>()
						  .Where(
							  x =>
									  x.Status == PaymentStatus.Active &&
									  x.Service == service
						  )
						  .ToList();
		}
		
		protected async Task _UpdateStatusAsync(string id, PaymentStatus status, DateTime date) {
			var payment = await context.FindAsync<Payment>(id);

			if (payment is null) {
				throw new ArgumentException($"Payment with id {id} not found", nameof(id));
			}
			
			payment.CompletedAt = date;
			payment.Status = status;

			await context.SaveChangesAsync();
		}
	}
}