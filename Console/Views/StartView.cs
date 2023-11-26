using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;

namespace Console.Views;

public class StartView : ArgsView
{
    public StartView(IRoot root) : base(root)
    {
    }

    [Command]
    public Task Launcher() => Task.Run(() => Root.PushRedirect<MainView>());

    [Command]
    public Task Settings() => Task.Run(() => Root.PushRedirect<SettingsView>());
}