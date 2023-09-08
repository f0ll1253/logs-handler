using Autofac;
using Console.Models;
using Console.Views;
using Core.LogParsers;
using Core.View;
using Core.View.Models.Abstractions;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        await App.Initialize(builder =>
        {
            builder.RegisterServices();
            builder.RegisterViews();
        });
        await App.Run(Locator.Current.GetService<StartView>()!);
    }

    private static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<LogsInfo>()
            .SingleInstance();
        builder.RegisterType<RedlineFactory>()
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