using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.User.Callbacks {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_logs_show")]
	public class AvailableLogsCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
		public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var data = update.data.Utf8().Split(':');
			var page = int.Parse(data[1]);
			var method = data[2];
			var back = data[3];

			if (Markup_General.AvailableLogsMarkup(method, back, page) is not { } markup) {
				return client.Messages_SetBotCallbackAnswer(
					update.query_id,
					0,
					Markup_General.AvailableLogsError
				);
			}

			return client.Messages_EditMessage(
				user,
				update.msg_id,
				Markup_General.AvailableLogsText,
				reply_markup: markup
			);
		}
	}
}