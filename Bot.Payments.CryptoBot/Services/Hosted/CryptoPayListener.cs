using Bot.Database;
using Bot.Models.Payments;
using Bot.Services.Abstractions;

using CryptoPay;
using CryptoPay.Types;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Payments.CryptoBot.Services.Hosted {
	public class CryptoPayListener(ICryptoPayClient pay, IPaymentsRepository<Invoice> payments, PaymentsDbContext context, ILogger<CryptoPayListener> logger) : IHostedService {
		public async Task StartAsync(CancellationToken cancellationToken) {
			for (;!cancellationToken.IsCancellationRequested; await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken)) {
				var invoice_ids = await context.Set<Payment>()
											   .Include(x => x.Data)
											   .Select(x => x.Data)
											   .OfType<CryptoPayPaymentData>()
											   .Select(x => x.InvoiceId)
											   .ToListAsync(cancellationToken: cancellationToken);

				if (!invoice_ids.Any()) {
					continue;
				}
				
				var invoices = await pay.GetInvoicesAsync(
					invoiceIds: invoice_ids,
					cancellationToken: cancellationToken
				);
				
				foreach (var invoice in invoices.Items) {
					if (invoice is null) {
						continue;
					}

					switch (invoice.Status) {
						case Statuses.paid:
							await payments.CompleteAsync(invoice.Hash, invoice.PaidAt!.Value);
							break;
						case Statuses.expired:
							await payments.ExpireAsync(invoice.Hash, invoice.ExpirationDate!.Value);
							break;
						case Statuses.active:
							break;
						default:
							logger.LogError("Unknown Status: {status}\nPayment id: {id}", invoice.Status.ToString(), invoice.Hash);
							break;
					}
				}
			}
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.CompletedTask; // ignore
		}
	}
}