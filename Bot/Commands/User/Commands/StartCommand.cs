using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.User.Commands;

[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = "/start", Duplicate = DuplicateStrategy.Append)]
[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_start")]
public class StartCommand(Client client) : ICommand<UpdateNewMessage>, ICommand<UpdateBotCallbackQuery> {
    public Task ExecuteAsync(UpdateNewMessage update, TL.User user) {
        return client.Messages_SendMessage(
            user,
            $"Hello, {user.username}!",
            Random.Shared.NextInt64(),
            reply_markup: Markup_User.StartMarkup("user_start")
        );
    }

    public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
        return client.Messages_EditMessage(
            user,
            update.msg_id,
            $"Hello, {user.username}!",
            reply_markup: Markup_User.StartMarkup("user_start")
        );
    }
}