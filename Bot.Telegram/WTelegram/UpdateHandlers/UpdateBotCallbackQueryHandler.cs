using Bot.Core.Messages.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers.Base;

using TL;

namespace Bot.Telegram.WTelegram.UpdateHandlers {
	public class UpdateBotCallbackQueryHandler(IServiceProvider provider) : BaseUpdateHandler<UpdateHandlerRequest, UpdateHandlerResponse, UpdateBotCallbackQuery, int>(provider) {
		protected override Task<int> GetKeyAsync(UpdateBotCallbackQuery update) {
			return Task.FromResult((int)update.data[0]);
		}

		protected override Task<long> GetUserIdAsync(UpdateBotCallbackQuery update) {
			return Task.FromResult(update.user_id);
		}
	}
}