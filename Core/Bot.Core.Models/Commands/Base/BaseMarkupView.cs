using Bot.Core.Models.Commands.Abstractions;

using TL;

using WTelegram;

namespace Bot.Core.Models.Commands.Base {
	public abstract class BaseMarkupView(Client client) : IMarkupView<UpdateNewMessage>, IMarkupView<UpdateBotCallbackQuery> {
		public async Task ExecuteAsync(UpdateNewMessage update, User user) {
			await client.Messages_SendMessage(
				user,
				await BuildMessage(update, user),
				Random.Shared.NextInt64(),
				reply_markup: await BuildMarkup(update, user)
			);
		}
		
		public async Task ExecuteAsync(UpdateBotCallbackQuery update, User user) {
			await client.Messages_EditMessage(
				user,
				update.msg_id,
				await BuildMessage(update, user) is {Length: > 0} text ? text : null,
				reply_markup: await BuildMarkup(update, user) is {rows.Length: > 0} markup ? markup : null
			);
		}

		public abstract Task<string> BuildMessage(UpdateNewMessage update, User user);
		
		public abstract Task<ReplyInlineMarkup> BuildMarkup(UpdateNewMessage update, User user);
		
		public virtual Task<string> BuildMessage(UpdateBotCallbackQuery update, User user) => Task.FromResult("");

		public virtual Task<ReplyInlineMarkup> BuildMarkup(UpdateBotCallbackQuery update, User user) => Task.FromResult(
			new ReplyInlineMarkup() {
				rows = []
			}
		);
	}
}