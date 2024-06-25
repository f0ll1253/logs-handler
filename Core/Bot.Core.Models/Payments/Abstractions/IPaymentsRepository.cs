using Bot.Core.Models.Abstractions;

namespace Bot.Core.Models.Payments.Abstractions {
	public interface IPaymentsRepository : IRepository<IPayment, string> {
		Task<bool> AddAsync(
			string id,
			long user_id,
			PaymentService service,
			string currency,
			double amount,
			DateTimeOffset? created_at = null,
			IPaymentData? data = null
		);
		
		IAsyncEnumerable<IPayment> NotCompleted<TData>() where TData : class, IPaymentData;
	}
}