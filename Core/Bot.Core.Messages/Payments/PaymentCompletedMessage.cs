using Bot.Core.Models.Payments.Abstractions;

namespace Bot.Core.Messages.Payments {
	public class PaymentCompletedMessage {
		public required IPayment Payment { get; init; }
		public required DateTime CompletionTime { get; init; }
		public required PaymentResultStatus Status { get; init; }
	}

	public enum PaymentResultStatus {
		Success,
		Fail
	}
}