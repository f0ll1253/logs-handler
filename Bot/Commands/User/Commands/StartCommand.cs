using Bot.Commands.Markups;
using Bot.Models.Abstractions;

namespace Bot.Commands.User.Commands;

[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = "/start", Duplicate = DuplicateStrategy.Append)]
public class StartCommand(Client client) : ICommand<UpdateNewMessage> {
    public Task ExecuteAsync(UpdateNewMessage message, TL.User user) {
        return client.Messages_SendMessage(
            user,
            $"Hello, {user.username}!",
            Random.Shared.NextInt64(),
            reply_markup: Markup_User.StartMarkup
        );
    }
}