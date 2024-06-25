using Bot.Core.Models.Payments.Abstractions;
using Bot.Payments.CryptoBot.Models;

using CryptoPay;

using Microsoft.Extensions.Hosting;

using SlimMessageBus;

namespace Bot.Payments.CryptoBot.Services.Hosted {
	public class CryptoPayListener(ICryptoPayClient pay, IPaymentsRepository repository, IMessageBus bus) : IHostedService {
		public async Task StartAsync(CancellationToken cancellationToken) {
			for (; !cancellationToken.IsCancellationRequested; await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken)) {
				var payments = repository.NotCompleted<CryptoPayPaymentData>();

				if (!await payments.AnyAsync(cancellationToken: cancellationToken)) {
					continue;
				}

				var invoices = await pay.GetInvoicesAsync(
					invoice_ids: await payments.Select(x => (x.Data as CryptoPayPaymentData).InvoiceId).ToArrayAsync(cancellationToken: cancellationToken),
					cancellation_token: cancellationToken
				);

				foreach (var invoice in invoices.Items) {
					if (invoice is null) {
						continue;
					}

					await bus.Publish(
						invoice.ToMessage(
							await payments.FirstAsync(
								x => x.Id == invoice.Hash,
								cancellationToken
							)
						),
						cancellationToken: cancellationToken
					);
				}
			}
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.CompletedTask; // ignore
		}
	}
}