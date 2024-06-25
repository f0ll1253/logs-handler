using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Bot.Core.Models.Payments.Abstractions;

using CryptoPay;
using CryptoPay.Types.Update;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SlimMessageBus;

namespace Bot.Payments.CryptoBot.Controllers {
	[ApiController, Route("/.webhooks/payments/cryptopay")]
	public class CryptoPayWebhook(IMessageBus bus, IPaymentsRepository repository, ILogger<CryptoPayWebhook> logger) : ControllerBase {
		[HttpPost("{token}")]
		public async Task<IActionResult> PostAsync([FromRoute] string token) {
			string content;

			using (var reader = new StreamReader(Request.Body, leaveOpen: true)) {
				content = await reader.ReadToEndAsync();
			}
			
			if (!HttpContext.Request.Headers.TryGetValue("crypto-pay-api-signature", out var signature) || !CryptoPayHelper.CheckSignature(signature, token, content)) {
				return Ok();
			}

			var update = JsonSerializer.Deserialize<Update>(content)!;

			var payment = await repository.GetAsync(update.Payload.Hash);

			if (payment is null) {
				logger.LogWarning("Payment {id} not found", update.Payload.Hash);

				return Ok();
			}

			await bus.Publish(
				update.Payload.ToMessage(
					payment
				)
			);

			return Ok();
		}
	}
}