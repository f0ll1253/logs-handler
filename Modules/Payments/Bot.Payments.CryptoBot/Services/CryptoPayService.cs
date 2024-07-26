using Bot.Core.Models.Payments.Abstractions;
using Bot.Payments.CryptoBot.Models;

using CryptoPay;
using CryptoPay.Types;
using CryptoPay.Types.Update;

namespace Bot.Payments.CryptoBot.Services {
	public class CryptoPayService(ICryptoPayClient pay, IPaymentsRepository repository) {
		public async Task<Invoice> CreateAsync(long user_id, string currency, double amount, string? description = null, string? return_callback = null) {
			var invoice = await pay.CreateInvoiceAsync(
				amount,
				asset: currency,
				description: description,
				paid_btn_name: return_callback is null ? null : PaidButtonNames.callback,
				paid_btn_url: return_callback,
				payload: user_id.ToString(),
				allow_comments: false,
				allow_anonymous: true,
				expires_in: (int)TimeSpan.FromMinutes(10).TotalSeconds
			);

			await repository.AddAsync(
				invoice.Hash,
				user_id,
				PaymentService.CryptoPay,
				invoice.Asset,
				invoice.Amount,
				invoice.CreatedAt,
				new CryptoPayPaymentData {
					InvoiceId = invoice.InvoiceId
				}
			);

			return invoice;
		}
	}
}