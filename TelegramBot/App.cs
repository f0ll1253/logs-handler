using System.Reflection;
using Serilog;
using Splat;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Models;

namespace TelegramBot;

public class App(ITelegramBotClient bot)
{
    public CancellationTokenSource Cancel { get; } = new();
    public IReadOnlyDictionary<string, (CommandsView, MethodInfo)> Commands { get; private set; }

    public Task Run()
    {
        InitializeViews();
        Log.Information("Views was initialized");
        
        var options = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: options,
            cancellationToken: Cancel.Token
        );
        
        Log.Information("Bot successfully started");

        while (true)
        {
            var cmd = Console.ReadLine();
            
            if (cmd == "exit") break;
        }

        Cancel.Cancel();
        
        return Task.CompletedTask;
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
    
    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return Task.CompletedTask;
        
        if (message.Text is not { } text)
            return Task.CompletedTask;

        if (text.StartsWith('/'))
        {
            var index = text.IndexOf(' ');
            var command = Commands[text[..(index == -1 ? text.Length : index)]];

            return (Task) command.Item2.Invoke(command.Item1, new [] {update});
        }
        
        return Task.CompletedTask;
    }
    
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Log.Error(errorMessage);
        return Task.CompletedTask;
    }
}