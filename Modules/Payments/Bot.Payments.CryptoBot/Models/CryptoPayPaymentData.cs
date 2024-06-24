using Bot.Core.Models.Payments.Abstractions;

namespace Bot.Payments.CryptoBot.Models {
	public class CryptoPayPaymentData : IPaymentData {
		public required long InvoiceId { get; set; }
		public string Id { get; } = Guid.NewGuid().ToString();

		public IPayment? Payment { get; set; }
		public string PaymentId { get; init; }
	}
}