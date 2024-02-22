using TelegramBot.Data;
using TelegramBot.Models;
using TL;
using WTelegram;
using User = TL.User;

namespace TelegramBot.Commands;

public class StartCommand(Client client, AppDbContext context) : ICommand
{
    public bool AuthorizedOnly { get; } = false;

    public async Task Invoke(UpdateNewMessage update, User user)
    {
        var dbuser = await context.FindAsync<Data.User>(update.message.Peer.ID);

        if (dbuser is not { IsApproved: true })
        {
            await client.Messages_SendMessage(user,
                $"Hello {user.first_name}! Please send your invite code. (/invite code)",
                new Random().NextInt64());
            return;
        }

        await client.Messages_SendMessage(user, $"Hello {user.first_name}!", new Random().NextInt64(),
            reply_markup: App.MainReplyKeyboardMarkup);
    }
}