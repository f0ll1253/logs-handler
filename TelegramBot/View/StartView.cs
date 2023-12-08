using TelegramBot.Data;
using TelegramBot.Models;
using TelegramBot.Models.Attributes;
using TL;
using WTelegram;

namespace TelegramBot.View;

public class StartView(Client client, AppDbContext context) : CommandsView
{
    [Command("/start")]
    public async Task Start(UpdateNewMessage update)
    {
        var user = await context.FindAsync<User>(update.message.Peer.ID);

        if (user is null)
        {
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], $"Hello {App.Users[update.message.Peer.ID].first_name}! Please send your invite code. (/invite code)", new Random().NextInt64());
            return;
        }
        
        await client.Messages_SendMessage(
            App.Users[update.message.Peer.ID], 
            $"Hello {App.Users[update.message.Peer.ID].first_name}!", 
            new Random().NextInt64(),
            reply_markup: new ReplyKeyboardMarkup
            {
                rows = new []
                {
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Cookies"
                            },
                            new KeyboardButton
                            {
                                text = "Links"
                            }
                        }
                    },
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Accounts"
                            }
                        }
                    }
                },
                flags = ReplyKeyboardMarkup.Flags.resize
            });
    }

    [Command("/invite")]
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
            id = message.peer_id.ID,
            IsApproved = true
        });
        await context.SaveChangesAsync();
        
        await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Invite code was successfully verified", new Random().NextInt64());
    }
}