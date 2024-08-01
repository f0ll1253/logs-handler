using System.Text;

using Bot.Core.Models.Commands.Abstractions;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.Common {
	public class ConfirmationCommand(Client client) : IManualCommand<ConfirmationArgs> {
		public Task ExecuteAsync(TL.User user, ConfirmationArgs args) {
			var bytes_metadata = Encoding.UTF8.GetBytes(args.Metadata);

			var markup = new ReplyInlineMarkup {
				rows = [
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "\u2705",
								data = [args.OnSuccess, ..bytes_metadata]
							},
							new KeyboardButtonCallback {
								text = "\u274c",
								data = [args.OnFail, ..bytes_metadata]
							}
						]
					}
				]
			};

			return args.MessageId.HasValue ?
					client.Messages_EditMessage(
						user,
						args.MessageId.Value,
						args.Message,
						reply_markup: markup
					) :
					client.Messages_SendMessage(
						user,
						args.Message,
						Random.Shared.NextInt64(),
						reply_markup: markup
					);
		}
	}

	public record ConfirmationArgs(int? MessageId, byte OnSuccess, byte OnFail, string Message, string Metadata);
}