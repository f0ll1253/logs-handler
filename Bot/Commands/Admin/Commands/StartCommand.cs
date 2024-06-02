using Bot.Commands.Markups;
using Bot.Data;
using Bot.Models.Abstractions;
using Bot.Models.Base;

namespace Bot.Commands.Admin.Commands;

[RegisterTransient<ICommand<UpdateNewMessage>>(ServiceKey = "/start", Duplicate = DuplicateStrategy.Append)]
[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "admin_start")]
public class StartCommand(Client client, UsersDbContext context) : RoleFilter(context, "Admin"), ICommand<UpdateNewMessage>, ICommand<UpdateBotCallbackQuery> {
    public override int Order { get; } = 0;
    
    public Task ExecuteAsync(UpdateNewMessage update, TL.User user) {
        return client.Messages_SendMessage(
            user,
            $"Hello admin, {user.username}!",
            Random.Shared.NextInt64(),
            reply_markup: Markup_Admin.StartMarkup
        );
    }

    public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
        return client.Messages_EditMessage(
            user,
            update.msg_id,
            $"Hello admin, {user.username}!",
            reply_markup: Markup_Admin.StartMarkup
        );
    }
}