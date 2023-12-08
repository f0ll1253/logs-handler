using TelegramBot.Data;
using TelegramBot.Models;
using TelegramBot.Models.Attributes;
using TL;
using WTelegram;
using User = TelegramBot.Data.User;

namespace TelegramBot.View;

public class StartView(Client client, AppDbContext context) : CommandsView
{
    [Command(Command = "/start")]
    public Task Start(UpdateNewMessage update)
    {
        return client.Messages_SendMessage(App.Users[update.message.Peer.ID], $"Hello {App.Users[update.message.Peer.ID].first_name}! Please send your invite code. (/invite code)", new Random().NextInt64());
    }

    [Command(Command = "/invite")]
    public async Task Invite(UpdateNewMessage update)
    {
        var message = (Message) update.message;
        var codeId = message.message.Split(' ')[1];
        var code = await context.FindAsync<InviteCode>(codeId);

        if (code is not { IsValid: true })
        {
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Invite code invalid", new Random().NextInt64());
            
            return;
        }

        if (code.Expire <= DateTime.UtcNow.Ticks)
        {
            code.IsValid = false;
            context.Update(code);
            await context.SaveChangesAsync();
            
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Invite code invalid", new Random().NextInt64());
            
            return;
        }

        await context.AddAsync(new User
        {
            id = message.from_id.ID,
            IsApproved = true
        });
        await context.SaveChangesAsync();
        
        await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Invite code was successfully verified", new Random().NextInt64());
    }
}