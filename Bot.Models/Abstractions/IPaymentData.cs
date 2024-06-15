using Bot.Models.Payments;

namespace Bot.Models.Abstractions {
	public interface IPaymentData : IEntity<string> {
		Payment? Payment { get; set; }
		string PaymentId { get; set; }
	}
}