using Console.Models;
using Core.View;
using Core.View.Attributes;
using Core.View.Models;
using Core.View.Models.Abstractions;

namespace Console.Views;

public class MainView : ArgsView
{
    private readonly LogsInfo _logs;
    
    public MainView(IRoot root, LogsInfo logs) : base(root)
    {
        _logs = logs;
    }
    
    [Command]
    private Task Parse_Wallets()
    {
        return Task.CompletedTask;
    }

    public override async Task Build()
    {
        System.Console.WriteLine("Count\tWith Passwords\tWallets\tSteam\tTelegram\n" +
                                 $"{await _logs.Logs.CountAsync()}" +
                                 $"\t{await _logs.Logs.CountAwaitAsync(async x => await x.Passwords.AnyAsync())}" +
                                 $"\t\t{await _logs.Logs.CountAsync(x => x.HasWallets)}" +
                                 $"\t{await _logs.Logs.CountAsync(x => x.HasSteam)}" +
                                 $"\t{await _logs.Logs.CountAsync(x => x.HasTelegram)}");
        System.Console.WriteLine();
        
        await base.Build();
    }
}