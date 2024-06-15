using Bot.Database;
using Bot.Models.Payments;
using Bot.Services.Abstractions;
using Bot.Services.Base;

using CryptoPay;
using CryptoPay.Types;

using Injectio.Attributes;

using Invoice = CryptoPay.Types.Invoice;

namespace Bot.Payments.CryptoBot.Services {
	[RegisterScoped<IPaymentsRepository<Invoice>>]
	public class CryptoPayPaymentsRepository(ICryptoPayClient pay, PaymentsDbContext context) : PaymentsRepositoryBase<Invoice>(context) {
		public override async Task<Invoice> CreateAsync(long user_id, string currency, double amount, string? description = null, string? return_callback = null) {
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

			await context.AddAsync(
				new Payment {
					Id = invoice.Hash,
					UserId = user_id,
					Service = PaymentService.CryptoPay,
					Currency = currency,
					Amount = amount,
					CreatedAt = DateTime.UtcNow,
					
					Data = new CryptoPayPaymentData {
						InvoiceId = invoice.InvoiceId
					}
				}
			);

			await context.SaveChangesAsync();

			return invoice;
		}
	}
}