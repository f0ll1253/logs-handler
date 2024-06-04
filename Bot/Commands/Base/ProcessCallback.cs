using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.Base {
	public abstract class ProcessCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public virtual Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var data = update.data.Utf8().Split(':');

			return client.Messages_EditMessage(
				user,
				update.msg_id,
				$"Parsing from {data[1]}\n{Markup_General.AvailableLogsText}",
				reply_markup: Markup_General.AvailableLogsMarkup(
					data[0],
					data[2],
					int.Parse(data[3])
				)
			);
		}
	}
}