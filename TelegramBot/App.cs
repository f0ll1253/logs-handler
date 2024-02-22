using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Splat;
using TelegramBot.Data;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TL;
using WTelegram;
using User = TL.User;

namespace TelegramBot;

public class App(Client client, IConfiguration config)
{
    private static readonly Regex _cmd = new("(.*?)($| |\n)");
    private static Dictionary<long, User> Users { get; } = new();
    private static Dictionary<long, ChatBase> Chats { get; } = new();

    public static Dictionary<long, Func<UpdateNewMessage, User, Task>> Hooks { get; } = new();
    public static User Me { get; private set; }

    public static ReplyKeyboardMarkup MainReplyKeyboardMarkup { get; } = new()
    {
        rows =
        [
            new KeyboardButtonRow
            {
                buttons =
                [
                    new KeyboardButton
                    {
                        text = "Cookies"
                    },
                    new KeyboardButton
                    {
                        text = "Services"
                    }
                ]
            }
        ],
        flags = ReplyKeyboardMarkup.Flags.resize
    };

    public async Task Run()
    {
        Log.Information("Views was initialized");

        App.Me = await client.LoginBotIfNeeded(config["Bot:Token"]!);
        client.OnUpdate += OnUpdate;
        Log.Information("Bot successfully started");

        while (true)
        {
            string? cmd = Console.ReadLine();

            if (cmd == "exit") break;
        }
    }

    private async Task OnUpdate(UpdatesBase updates)
    {
        updates.CollectUsersChats(App.Users, App.Chats);

        foreach (var update in updates.UpdateList)
            switch (update)
            {
                case UpdateNewMessage msg:
                    await HandleUpdateAsync(msg);
                    break;
                case UpdateBotCallbackQuery msg:
                    await HandleCallbackAsync(msg);
                    break;
            }
    }

    private async Task HandleCallbackAsync(UpdateBotCallbackQuery update)
    {
        var messages = await client.Messages_GetMessages(update.msg_id);

        if (messages.Messages.FirstOrDefault() is not Message message) return;

        if (Encoding.UTF8.GetString(update.data) == "Cancel")
        {
            await App.RemoveHook(update.user_id, update.msg_id, client);
            return;
        }

        if (Locator.Current.GetService<ICallbackCommand>(_cmd.Match(message.message).Groups[1].Value) is not
            { } cmd) return;

        await cmd.Invoke(update, App.Users[update.user_id]);
    }

    private async Task HandleUpdateAsync(UpdateNewMessage update)
    {
        if (update.message is not Message message)
            return;

        var random = Locator.Current.GetService<Random>()!;

        if (App.Hooks.TryGetValue(message.Peer.ID, out var func))
        {
            if (message.message == "Cancel")
                await App.RemoveHook(message.Peer.ID, message.ID, client);
            else
                await func.Invoke(update, App.Users[message.Peer.ID]);

            return;
        }

        int index = message.message.IndexOf(' ');
        string? command = message.message[..(index == -1 ? message.message.Length : index)];
        var user = await Locator.Current.GetService<AppDbContext>()!.FindAsync<Data.User>(message.Peer.ID);

        if (Locator.Current.GetService<ICommand>(command == "Back" ? "/start" : command) is not { } cmd)
        {
            await client.Messages_SendMessage(App.Users[message.Peer.ID], "Command not found",
                Random.Shared.NextInt64(), reply_markup: App.MainReplyKeyboardMarkup);
            return;
        }

        if (cmd.AuthorizedOnly && user is not { IsApproved: true })
        {
            await client.Messages_SendMessage(App.Users[message.Peer.ID], "Permission denied",
                Random.Shared.NextInt64());
            return;
        }

        await cmd.Invoke(update, App.Users[message.Peer.ID]);
    }

    // Helpers
    private static Task RemoveHook(long userId, int messageId, Client client)
    {
        App.Hooks.Remove(userId);
        return client.EditMessage(App.Users[userId], messageId, "Action canceled", App.MainReplyKeyboardMarkup);
    }
}