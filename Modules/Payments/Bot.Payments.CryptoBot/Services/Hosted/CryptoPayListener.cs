using Bot.Core.Messages.Payments;
using Bot.Core.Models.Payments.Abstractions;
using Bot.Payments.CryptoBot.Models;

using CryptoPay;
using CryptoPay.Types;

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
					invoiceIds: await payments.Select(x => (x.Data as CryptoPayPaymentData).InvoiceId).ToArrayAsync(cancellationToken: cancellationToken),
					cancellationToken: cancellationToken
				);

				foreach (var invoice in invoices.Items) {
					if (invoice is null) {
						continue;
					}

					await bus.Publish(
						new PaymentCompletedMessage {
							Payment = await payments.FirstAsync(
								x => (x.Data as CryptoPayPaymentData).InvoiceId == invoice.InvoiceId,
								cancellationToken
							),
							CompletionTime = invoice.Status switch {
								Statuses.paid    => invoice.PaidAt!.Value,
								Statuses.expired => invoice.ExpirationDate!.Value
							},
							Status = invoice.Status switch {
								Statuses.paid    => PaymentResultStatus.Success,
								Statuses.expired => PaymentResultStatus.Fail
							}
						}
					);
				}
			}
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.CompletedTask; // ignore
		}
	}
}