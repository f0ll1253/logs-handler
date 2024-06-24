namespace Bot.Core.Models.Payments.Abstractions {
	public interface IPayment {
		string Id { get; }
		long UserId { get; init; }
		PaymentService Service { get; init; }

		string Currency { get; init; }
		double Amount { get; init; }
		DateTime CreatedAt { get; init; }

		DateTime? CompletedAt { get; set; }
		PaymentStatus Status { get; set; }

		IPaymentData? Data { get; set; }
	}

	public enum PaymentService {
		CryptoPay = 0
	}

	public enum PaymentStatus {
		Active = 0,
		Expired = 1,
		Paid = 2
	}
}