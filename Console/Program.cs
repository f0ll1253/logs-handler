using Autofac;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Views;
using Console.Views;
using Core.Checkers;
using Core.Checkers.Crypto;
using Core.Models;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        System.Console.Title = "https://github.com/f0ll1253";

        ThreadPool.SetMaxThreads(1000 + 10, 1000);
        
        Task.Run(() =>
        {
            while (true)
            {
                ThreadPool.GetAvailableThreads(out var worker, out var port);
                
                System.Console.Title = $"{worker} | {port}";
            }
        });
        
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
            .SingleInstance()
            .AsSelf();
        builder.RegisterType<DiscordChecker>()
            .SingleInstance()
            .AsSelf();
        builder.RegisterType<IGVChecker>()
            .SingleInstance()
            .AsSelf();
        builder.RegisterType<CatmineChecker>()
            .SingleInstance()
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