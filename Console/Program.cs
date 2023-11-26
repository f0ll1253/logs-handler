using Autofac;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Views;
using Console.Views;
using Core.Discord;
using Core.IGV;
using Core.Models;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        System.Console.Title = "https://github.com/f0ll1253";
        
        await App.Initialize(builder =>
        {
            builder.RegisterServices();
            builder.RegisterViews();
        });
        
        await App.Run(Locator.Current.GetService<StartView>()!);
    }

    private static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<Settings>()
            .OnActivated(x =>
            {
                x.Instance.Initialize();

                if (!Directory.Exists(x.Instance.Path)) x.Instance.Path = "";
            })
            .SingleInstance()
            .AsSelf();
        builder.RegisterType<ProxyPool>()
            .OnActivated(x =>
            {
                var settings = x.Context.Resolve<Settings>();

                if (!File.Exists(settings.ProxyPath))
                {
                    settings.ProxyPath = "";
                    return;
                }

                x.Instance.LoadAsync(settings.ProxyPath).GetAwaiter().GetResult();
            })
            .SingleInstance()
            .AsSelf();
        
        builder.RegisterType<DataService>()
            .AsSelf();
        builder.RegisterType<DiscordChecker>()
            .AsSelf();
        builder.RegisterType<IGVChecker>()
            .AsSelf();
    }
    
    private static void RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<Root>()
            .As<IRoot>()
            .SingleInstance();

        builder.RegisterType<StartView>();
        builder.RegisterType<MainView>();
        builder.RegisterType<SettingsView>();
        builder.RegisterType<ServicesView>();
    }
}