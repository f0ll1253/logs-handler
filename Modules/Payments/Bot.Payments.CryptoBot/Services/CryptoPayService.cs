using Bot.Core.Models.Payments.Abstractions;
using Bot.Payments.CryptoBot.Models;

using CryptoPay;
using CryptoPay.Types;

using Injectio.Attributes;

using Invoice = CryptoPay.Types.Invoice;

namespace Bot.Payments.CryptoBot.Services {
	[RegisterTransient]
	public class CryptoPayService(ICryptoPayClient pay, IPaymentsRepository repository) {
		public async Task<Invoice> CreateAsync(long user_id, string currency, double amount, string? description = null, string? return_callback = null) {
			var invoice = await pay.CreateInvoiceAsync(
				amount,
				asset: currency,
				description: description,
				paidBtnName: return_callback is null ? null : PaidButtonNames.callback,
				paidBtnUrl: return_callback,
				payload: user_id.ToString(),
				allowComments: false,
				allowAnonymous: true,
				expiresIn: (int)TimeSpan.FromMinutes(10).TotalSeconds
			);

			await repository.AddAsync(
				invoice.Hash,
				user_id,
				PaymentService.CryptoPay,
				invoice.Asset,
				invoice.Amount,
				invoice.CreatedAt,
				
				data: new CryptoPayPaymentData {
					InvoiceId = invoice.InvoiceId
				}
			);

			return invoice;
		}
	}
}