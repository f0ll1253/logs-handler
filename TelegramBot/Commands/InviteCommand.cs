using TelegramBot.Data;
using TelegramBot.Models;
using TL;
using WTelegram;
using User = TL.User;

namespace TelegramBot.Commands;

public class InviteCommand(Client client, AppDbContext context) : ICommand
{
    public bool AuthorizedOnly { get; } = false;

    public async Task Invoke(UpdateNewMessage update, User user)
    {
        var message = (Message)update.message;
        string? codeId = message.message.Split(' ').ElementAtOrDefault(1);

        if (codeId is null)
        {
            await client.Messages_SendMessage(user, "Code id not found", Random.Shared.NextInt64());

            return;
        }

        var code = await context.FindAsync<InviteCode>(codeId);

        if (code is not { IsValid: true })
        {
            await client.Messages_SendMessage(user, "Invite code invalid", Random.Shared.NextInt64());

            return;
        }

        if (code.Expire <= DateTime.UtcNow.Ticks)
        {
            code.IsValid = false;
            context.Update(code);
            await context.SaveChangesAsync();

            await client.Messages_SendMessage(user, "Invite code invalid", Random.Shared.NextInt64());

            return;
        }

        await context.AddAsync(new Data.User
        {
            id = message.peer_id.ID,
            IsApproved = true
        });
        await context.SaveChangesAsync();

        await client.Messages_SendMessage(user, "Invite code was successfully verified", Random.Shared.NextInt64());
    }
}