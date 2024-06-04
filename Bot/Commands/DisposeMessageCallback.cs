using Bot.Models.Abstractions;

namespace Bot.Commands;

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "dispose")]
public class DisposeMessageCallback(Client client) : ICommand<UpdateBotCallbackQuery> {
    public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
        return client.Messages_DeleteMessages([update.msg_id], true);
    }
}