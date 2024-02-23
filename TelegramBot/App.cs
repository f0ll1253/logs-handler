using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Splat;
using TelegramBot.Data;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;
using User = TL.User;

namespace TelegramBot;

public partial class App(Client client, IConfiguration config, DataService data)
{
    private static readonly Regex _cmd = new("(.*?)($| |\n)");
    private static Dictionary<long, User> Users { get; } = new();
    private static Dictionary<long, ChatBase> Chats { get; } = new();

    public async Task Run()
    {
        Log.Information("Views was initialized");

        await client.LoginBotIfNeeded(config["Bot:Token"]!);
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

        if (Locator.Current.GetService<ICallbackCommand>(_cmd.Match(message.message).Groups[1].Value) is not { } cmd)
            return;

        await cmd.Invoke(update, App.Users[update.user_id]);
    }

    private async Task HandleUpdateAsync(UpdateNewMessage update)
    {
        if (update.message is not Message message)
            return;

        if (message is { flags: Message.Flags.has_media, media: MessageMediaDocument { document: Document document } })
        {
            string? filepath = await data.DownloadDocumentAsync(document);

            if (filepath is null) return;

            string? password =
                App.RegexPassword()
                   .Match(((Message)update.message).message).Groups[4]
                    is { Success: true, Value: not "none" and "\u2796" } result
                    ? result.Value
                    : null;

            data.ExtractFiles(filepath, password);
            
            return;
        }

        int index = message.message.IndexOf(' ');
        string? command = message.message[..(index == -1 ? message.message.Length : index)];
        var user = await Locator.Current.GetService<AppDbContext>()!.FindAsync<Data.User>(message.Peer.ID);

        if (Locator.Current.GetService<ICommand>(command == "Back" ? "/start" : command) is not { } cmd)
            return;

        if (cmd.AuthorizedOnly && user is not { IsApproved: true })
        {
            await client.Messages_SendMessage(App.Users[message.Peer.ID], "Permission denied",
                Random.Shared.NextInt64());
            return;
        }

        await cmd.Invoke(update, App.Users[message.Peer.ID]);
    }

    [GeneratedRegex(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)")]
    private static partial Regex RegexPassword();
}