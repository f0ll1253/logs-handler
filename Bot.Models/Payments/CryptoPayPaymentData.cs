using Bot.Models.Abstractions;

namespace Bot.Models.Payments {
	public class CryptoPayPaymentData : IPaymentData {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		
		public Payment? Payment { get; set; }
		public string PaymentId { get; set; }
		
		public required long InvoiceId { get; set; }
	}
}