using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Splat;
using TelegramBot.Data;
using TelegramBot.Models;
using TL;
using WTelegram;
using User = TL.User;

namespace TelegramBot;

public class App(Client client, IConfiguration config)
{
    private static Dictionary<long, User> Users { get; } = new();
    private static Dictionary<long, ChatBase> Chats { get; } = new();
    
    public static Dictionary<long, Func<UpdateNewMessage, User, Task>> Default { get; } = new();
    public static User Me { get; private set; }

    public static ReplyKeyboardMarkup MainReplyKeyboardMarkup { get; } = new()
    {
        rows = new[]
        {
            new KeyboardButtonRow
            {
                buttons = new[]
                {
                    new KeyboardButton
                    {
                        text = "Cookies"
                    },
                    new KeyboardButton
                    {
                        text = "Services"
                    },
                }
            }
        },
        flags = ReplyKeyboardMarkup.Flags.resize
    };

    public static ReplyKeyboardMarkup CancelReplyKeyboardMarkup { get; } = new()
    {
        rows = new[]
        {
            new KeyboardButtonRow
            {
                buttons = new[]
                {
                    new KeyboardButton
                    {
                        text = "Cancel"
                    }
                }
            }
        },
        flags = ReplyKeyboardMarkup.Flags.resize
    };

    public async Task Run()
    {
        Log.Information("Views was initialized");

        Me = await client.LoginBotIfNeeded(config["Bot:Token"]!);
        client.OnUpdate += OnUpdate;
        Log.Information("Bot successfully started");

        while (true)
        {
            var cmd = Console.ReadLine();
            
            if (cmd == "exit") break;
        }
    }

    private async Task OnUpdate(UpdatesBase updates)
    {
        updates.CollectUsersChats(Users, Chats);
        
        foreach (var update in updates.UpdateList)
        {
            switch (update)
            {
                case UpdateNewMessage msg: await HandleUpdateAsync(msg); break;
                case UpdateBotCallbackQuery msg: await HandleCallbackAsync(msg); break;
            }
        }
    }

    private static readonly Regex _cmd = new("(.*?)($| |\n)");
    private async Task HandleCallbackAsync(UpdateBotCallbackQuery update)
    {
        var messages = await client.Messages_GetMessages(update.msg_id);
        
        if (messages.Messages.FirstOrDefault() is not Message message) return;

        if (Locator.Current.GetService<ICallbackCommand>(_cmd.Match(message.message).Groups[1].Value) is not {} cmd) return;

        await cmd.Invoke(update, Users[update.user_id]);
    }
    
    private async Task HandleUpdateAsync(UpdateNewMessage update)
    {
        if (update.message is not Message message)
            return;
        
        var random = Locator.Current.GetService<Random>()!;
        
        if (Default.TryGetValue(update.message.Peer.ID, out var func))
        {
            if (message.message == "Cancel")
            {
                Default.Remove(update.message.Peer.ID);
                await client.Messages_SendMessage(Users[message.Peer.ID], "Action canceled", random.NextInt64(), reply_markup: MainReplyKeyboardMarkup);
            }
            else 
                await func.Invoke(update, Users[message.Peer.ID]);
            
            return;
        }

        var index = message.message.IndexOf(' ');
        var command = message.message[..(index == -1 ? message.message.Length : index)];
        var user = await Locator.Current.GetService<AppDbContext>()!.FindAsync<Data.User>(message.Peer.ID);

        if (Locator.Current.GetService<ICommand>(command == "Back" ? "/start" : command) is not { } cmd)
        {
            await client.Messages_SendMessage(Users[message.Peer.ID], "Command not found", random.NextInt64(), reply_markup: MainReplyKeyboardMarkup);
            return;
        }

        if (cmd.AuthorizedOnly && user is not { IsApproved: true })
        {
            await client.Messages_SendMessage(Users[message.Peer.ID], "Permission denied", random.NextInt64());
            return;
        }
        
        await cmd.Invoke(update, Users[message.Peer.ID]);
    }
}