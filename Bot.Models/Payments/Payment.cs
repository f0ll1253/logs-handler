using Bot.Models.Abstractions;

namespace Bot.Models.Payments {
	public class Payment : IEntity<string> {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public required long UserId { get; set; }
		public required PaymentService Service { get; set; }
		
		public required string Currency { get; set; }
		public required double Amount { get; set; }
		public required DateTime CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public PaymentStatus Status { get; set; } = PaymentStatus.Active;
		
		public IPaymentData? Data { get; set; }
		public string? DataId { get; set; }
	}

	public enum PaymentService {
		CryptoPay = 0,
	}
    
	public enum PaymentStatus {
		Active = 0,
		Expired = 1,
		Paid = 2,
	}
}