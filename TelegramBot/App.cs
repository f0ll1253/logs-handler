using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Splat;
using TelegramBot.Models;
using TL;
using WTelegram;

namespace TelegramBot;

public class App(Client client, IConfiguration config)
{
    public static Dictionary<long, Func<UpdateNewMessage, Task>> Wait { get; } = new();
    public static Dictionary<long, TL.User> Users { get; } = new();
    public static Dictionary<long, ChatBase> Chats { get; } = new();
    public static IReadOnlyDictionary<string, (CommandsView, MethodInfo)> Commands { get; private set; }
    public static TL.User Me { get; private set; }

    public async Task Run()
    {
        InitializeViews();
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

    private void InitializeViews()
    {
        var views = Locator.Current.GetServices<CommandsView>();

        foreach (var view in views)
        {
            view.Initialize();
        }

        Commands = views
            .SelectMany(x => x.Commands.ToDictionary(a => a.Key, a => (x, a.Value)))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task OnUpdate(UpdatesBase updates)
    {
        updates.CollectUsersChats(Users, Chats);
        
        foreach (var update in updates.UpdateList)
        {
            switch (update)
            {
                case UpdateNewMessage msg: await HandleUpdateAsync(msg); break;
            }
        }
    }
    
    private Task HandleUpdateAsync(UpdateNewMessage update)
    {
        if (Wait.TryGetValue(update.message.Peer.ID, out var func))
            return func.Invoke(update);
        
        if (update.message is not Message message)
            return Task.CompletedTask;

        var index = message.message.IndexOf(' ');
        var command = message.message[..(index == -1 ? message.message.Length : index)];

        if (Commands.TryGetValue(command, out var cmd))
            return (Task) cmd.Item2.Invoke(cmd.Item1, new[] { update });
        
        return Task.CompletedTask;
    }
}