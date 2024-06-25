using Bot.Core.Messages.Payments;
using Bot.Core.Models.Payments.Abstractions;

using CryptoPay.Types;
using CryptoPay.Types.Update;

namespace Bot.Payments.CryptoBot {
	internal static class Extensions {
		public static PaymentCompletedMessage ToMessage(this Invoice invoice, IPayment found) {
			return new() {
				Payment = found,
				CompletionTime = invoice.Status switch {
					Statuses.paid    => invoice.PaidAt,
					Statuses.expired => invoice.ExpirationDate
				},
				Status = invoice.Status switch {
					Statuses.paid    => PaymentResultStatus.Success,
					Statuses.expired => PaymentResultStatus.Fail
				}
			};
		}
	}
}