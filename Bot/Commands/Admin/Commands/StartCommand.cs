using Bot.Commands.Markups;
using Bot.Data;
using Bot.Models.Abstractions;
using Bot.Models.Base;

namespace Bot.Commands.Admin.Commands;

[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = "/start", Duplicate = DuplicateStrategy.Append)]
public class StartCommand(Client client, UsersDbContext context) : RoleFilter(context, "Admin"), ICommand<UpdateNewMessage> {
    public override int Order { get; set; } = 0;
    
    public Task ExecuteAsync(UpdateNewMessage message, TL.User user) {
        return client.Messages_SendMessage(
            user,
            $"Hello admin, {user.username}!",
            Random.Shared.NextInt64(),
            reply_markup: Markup_Admin.Start
        );
    }
}