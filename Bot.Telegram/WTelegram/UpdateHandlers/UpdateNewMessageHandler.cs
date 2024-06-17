using Bot.Core.Messages.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers.Abstractions;

using TL;

namespace Bot.Telegram.WTelegram.UpdateHandlers {
	public class UpdateNewMessageHandler(IServiceProvider provider) : BaseUpdateHandler<UpdateHandlerRequest, UpdateHandlerResponse>(provider) {
		public override async Task<UpdateHandlerResponse> OnHandle(UpdateHandlerRequest request) {
			if (request.Update is not UpdateNewMessage {message: Message {peer_id.ID: var id, message: var message}}) {
				return new();
			}

			return new() {
				Commands = await GetCommands<UpdateNewMessage>(ParseKey(message, ' '), request.UpdateManager.Users[id]).ToListAsync(),
				User = request.UpdateManager.Users[id]
			};
		}
	}
}