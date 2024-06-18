using Bot.Core.Messages.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers.Base;

using TL;

namespace Bot.Telegram.WTelegram.UpdateHandlers {
	public class UpdateNewMessageHandler(IServiceProvider provider) : BaseUpdateHandler<UpdateHandlerRequest, UpdateHandlerResponse, UpdateNewMessage, string>(provider) {
		protected override Task<string> GetKeyAsync(UpdateNewMessage update) {
			var text = ((Message)update.message).message;
			var index = text.IndexOf(' ');
			
			return Task.FromResult(text[..(index == -1 ? text.Length : index)]);
		}

		protected override Task<long> GetUserIdAsync(UpdateNewMessage update) => Task.FromResult(((Message)update.message).peer_id.ID);
	}
}