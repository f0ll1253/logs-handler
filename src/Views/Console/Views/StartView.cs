using Console.Models;
using Core.Models.Logs;
using Core.View.Models;
using Core.View.Models.Abstractions;
using Splat;

namespace Console.Views;

public class StartView : BaseView
{
    public StartView(IRoot root) : base(root)
    {
    }

    public override Task Build()
    {
        System.Console.Write("Path to logs: ");
        
        var path = System.Console.ReadLine();

        if (string.IsNullOrEmpty(path)
            || !Directory.Exists(path)) 
            return Task.CompletedTask;

        var logs = Locator.Current.GetService<LogsInfo>()!;
        var factory = Locator.Current.GetService<RedlineFactory>()!;
        logs.Path = path;
        logs.Logs = factory.CreateMany(path);

        Root.Views.Pop();
        Root.Views.Push(Locator.Current.GetService<MainView>()!);

        return Task.CompletedTask;
    }
}