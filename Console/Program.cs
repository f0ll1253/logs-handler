using Autofac;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Views;
using Console.Views;
using Microsoft.Extensions.Configuration;
using Nethereum.Util;
using Nethereum.Web3;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        System.Console.Title = "https://github.com/f0ll1253";

        App.ConfigureLogging();
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
            .SingleInstance();
    }
    
    private static void RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<Root>()
            .As<IRoot>()
            .SingleInstance();

        builder.RegisterType<StartView>();
        builder.RegisterType<MainView>();
    }
}