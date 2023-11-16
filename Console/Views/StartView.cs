using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Views;
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

        Locator.Current.GetService<Settings>()!.Path = path;

        Root.Views.Push(Locator.Current.GetService<MainView>()!);

        return Task.CompletedTask;
    }
}