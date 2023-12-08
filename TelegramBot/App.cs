using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Splat;
using TelegramBot.Models;
using TL;
using WTelegram;

namespace TelegramBot;

public class App(Client bot, IConfiguration config)
{
    public static Dictionary<long, User> Users { get; private set; } = new();
    public static Dictionary<long, ChatBase> Chats { get; private set; } = new();
    public static IReadOnlyDictionary<string, (CommandsView, MethodInfo)> Commands { get; private set; }
    public static User Me { get; private set; }

    public async Task Run()
    {
        InitializeViews();
        Log.Information("Views was initialized");

        Me = await bot.LoginBotIfNeeded(config["Bot:Token"]!);
        bot.OnUpdate += OnUpdate;
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
        if (update.message is not Message message)
            return Task.CompletedTask;

        if (message.message.StartsWith('/'))
        {
            var index = message.message.IndexOf(' ');
            var command = Commands[message.message[..(index == -1 ? message.message.Length : index)]];

            return (Task) command.Item2.Invoke(command.Item1, new [] {update});
        }
        
        return Task.CompletedTask;
    }
}