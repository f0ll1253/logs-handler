using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Models;

namespace Console.Views;

public class SettingsView : ArgsView
{
    private readonly Settings _settings;
    private readonly ProxyPool _proxy;
    
    public SettingsView(IRoot root, Settings settings, ProxyPool proxy) : base(root)
    {
        _settings = settings;
        _proxy = proxy;
    }

    public override void Dispose()
    {
        base.Dispose();
        _settings.Save();
    }

    [Command]
    public Task Set_Path()
    {
        System.Console.WriteLine($"Current: {_settings.Path}");
        System.Console.Write("Path to logs: ");
        
        var path = System.Console.ReadLine()?.Replace("\"", "");

        if (string.IsNullOrEmpty(path)
            || !Directory.Exists(path)) 
            return Task.CompletedTask;

        _settings.Path = path;

        return Task.CompletedTask;
    }
    
    [Command]
    private Task Set_Proxies()
    {
        System.Console.WriteLine($"Current: {_settings.ProxyPath}");
        System.Console.Write("Path to proxies: ");
        var proxiesPath =  System.Console.ReadLine()?.Replace("\"", "");

        if (string.IsNullOrEmpty(proxiesPath)) return Task.CompletedTask;
        
        _settings.ProxyPath = proxiesPath;

        return _proxy.LoadAsync(proxiesPath);
    }
}