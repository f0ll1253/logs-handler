namespace Bot.Core.Models.Payments.Abstractions {
	public interface IPaymentData {
		string Id { get; }

		IPayment? Payment { get; set; }
		string PaymentId { get; init; }
	}
}