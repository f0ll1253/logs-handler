using Bot.Models.Payments;

namespace Bot.Services.Abstractions {
	public interface IPaymentsRepository<TInvoice> where TInvoice : class {
		Task<TInvoice> CreateAsync(long user_id, string currency, double amount, string? description = null, string? return_callback = null);
		Task ExpireAsync(string id, DateTime date);
		Task CompleteAsync(string id, DateTime date);
		
		// CRUD
		ICollection<Payment> ActivePaymentsAsync(PaymentService service);
	}
}