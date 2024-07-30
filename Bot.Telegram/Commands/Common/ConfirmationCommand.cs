using System.Text;

using Bot.Core.Models.Commands.Abstractions;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.Common {
	public class ConfirmationCommand : IManualCommand<ConfirmationArgs> {
		public required Client Client { get; init; }
		
		public Task ExecuteAsync(TL.User user, ConfirmationArgs args) {
			var bytes_metadata = Encoding.UTF8.GetBytes(args.Metadata);
			
			return Client.Messages_EditMessage(
				user,
				args.MessageId,
				args.Message,
				reply_markup: new ReplyInlineMarkup {
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
				}
			);
		}
	}

	public record ConfirmationArgs(int MessageId, byte OnSuccess, byte OnFail, string Message, string Metadata);
}