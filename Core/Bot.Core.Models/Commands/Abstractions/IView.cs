using TL;

namespace Bot.Core.Models.Commands.Abstractions {
	public interface IView<in TUpdate> : ICommand<TUpdate> where TUpdate : Update {
		Task<string> BuildMessage(TUpdate update, User user);
		Task<ReplyInlineMarkup?> BuildMarkup(TUpdate update, User user);
	}
}