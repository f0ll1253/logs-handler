using Bot.Commands.Markups;
using Bot.Data;
using Bot.Models.Abstractions;
using Bot.Models.Base;

namespace Bot.Commands.Admin.Callbacks;

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "admin_settings")]
public class SettingsCallback(Client client, UsersDbContext users) : RoleFilter(users, "Admin"), ICommand<UpdateBotCallbackQuery> {
    public override int Order { get; } = 0;

    public Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
        return client.Messages_EditMessage(
            user,
            update.msg_id,
            reply_markup: Markup_Admin.SettingsMarkup
        );
    }
}