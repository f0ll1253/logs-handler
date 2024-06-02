using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.Base;

public abstract class AvailableLogsCallback(
    string showing_method,
    string method,
    
    // Inject
    Client client) : ICommand<UpdateBotCallbackQuery> {
    public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
        var data = update.data.Utf8().Split(':');
        var page = int.Parse(data[1]);
        var back = data[2];

        if (Markup_General.AvailableLogsMarkup(showing_method, method, back_method: back, page: page) is not { } markup) {
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