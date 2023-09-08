using Console.Models;
using Core.LogParsers;
using Core.View.Models.Abstractions;
using Core.View.Models.Realizations;
using Splat;

namespace Console.Views;

public class StartView : BaseView
{
    public StartView(IRoot root) : base(root)
    {
    }

    public override async Task Build()
    {
        System.Console.Write("Path to logs: ");
        
        var path = System.Console.ReadLine();

        if (string.IsNullOrEmpty(path)
            || !Directory.Exists(path)) 
            return;

        var logs = Locator.Current.GetService<LogsInfo>()!;
        var factory = Locator.Current.GetService<RedlineFactory>()!;
        logs.Path = path;
        logs.Logs.AddRange(await factory.CreateMany(path).ToListAsync());

        Root.Views.Pop();
        Root.Views.Push(Locator.Current.GetService<MainView>()!);
    }
}