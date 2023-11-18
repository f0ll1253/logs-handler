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

    public override void Activate()
    {
        if (Locator.Current.GetService<Settings>() is {} settings) settings.Path = "";
    }

    public override Task Build()
    {
        System.Console.Write("Path to logs: ");
        
        var path = System.Console.ReadLine()?.Replace("\"", "");

        if (string.IsNullOrEmpty(path)
            || !Directory.Exists(path)) 
            return Task.CompletedTask;

        Locator.Current.GetService<Settings>()!.Path = path;

        Root.PushRedirect<MainView>();

        return Task.CompletedTask;
    }
}