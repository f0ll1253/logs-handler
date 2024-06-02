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
        var page = data.Length == 1 ? 0 : int.Parse(data[1]);

        if (Markup_General.AvailableLogsMarkup(showing_method, method, page: page) is not { } markup) {
            return client.Messages_SetBotCallbackAnswer(
                update.query_id,
                0,
                Markup_General.AvailableLogsError
            );
        }
        
        return client.Messages_SendMessage(
            user,
            Markup_General.AvailableLogsText,
            Random.Shared.NextInt64(),
            reply_markup: markup
        );
    }
}