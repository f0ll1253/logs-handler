using Bot.Core.Models.Commands.Abstractions;

using TL;

using WTelegram;

namespace Bot.Core.Models.Commands.Base {
	public abstract class BaseView(Client client) : IView<UpdateNewMessage>, IView<UpdateBotCallbackQuery> {
		public async Task ExecuteAsync(UpdateNewMessage update, User user) {
			await client.Messages_SendMessage(
				user,
				await BuildMessage(update, user),
				Random.Shared.NextInt64(),
				reply_markup: await BuildMarkup(update, user)
			);
		}
		
		public async Task ExecuteAsync(UpdateBotCallbackQuery update, User user) {
			var message = (Message)(await client.GetMessages(user, update.msg_id)).Messages[0];
			var @new = (await BuildMessage(update, user), await BuildMarkup(update, user));

			if (@new.Item1.Length == 0 &&
				@new.Item2 == null ||
				message.message.Equals(@new.Item1) &&
				message.reply_markup.Equals(@new.Item2)
			   ) {
				return;
			}

			await client.Messages_EditMessage(
				user,
				update.msg_id,
				@new.Item1 is {Length: > 0} ? @new.Item1 : null,
				reply_markup: @new.Item2 ?? message.reply_markup
			);
		}

		public abstract Task<string> BuildMessage(UpdateNewMessage update, User user);
		
		public abstract Task<ReplyInlineMarkup?> BuildMarkup(UpdateNewMessage update, User user);
		
		public virtual Task<string> BuildMessage(UpdateBotCallbackQuery update, User user) => Task.FromResult("");

		public virtual Task<ReplyInlineMarkup?> BuildMarkup(UpdateBotCallbackQuery update, User user) => Task.FromResult<ReplyInlineMarkup?>(null);
	}
}