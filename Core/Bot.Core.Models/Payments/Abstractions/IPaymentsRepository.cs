using Bot.Core.Models.Abstractions;

namespace Bot.Core.Models.Payments.Abstractions {
	public interface IPaymentsRepository : IRepository<IPayment> {
		Task<bool> AddAsync(
			string id,
			long user_id,
			PaymentService service,
			string currency,
			double amount,
			DateTime? created_at = null,
			IPaymentData? data = null
		);
		
		IAsyncEnumerable<IPayment> NotCompleted<TData>() where TData : class, IPaymentData;
	}
}