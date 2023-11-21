using Autofac;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Views;
using Console.Views;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        System.Console.Title = "https://github.com/f0ll1253";

        ThreadPool.SetMaxThreads(500, 5000);

        Task.Run(() =>
        {
            while (true)
            {
                ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);

                System.Console.Title = $"{ThreadPool.ThreadCount} | {workerThreads} | {completionPortThreads}";
            }
        });
        
        App.ConfigureLogging();
        App.InitializeFiles();
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
        builder.RegisterType<SaverService>();
    }
    
    private static void RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<Root>()
            .As<IRoot>()
            .SingleInstance();

        builder.RegisterType<StartView>();
        builder.RegisterType<MainView>();
        builder.RegisterType<ServicesView>();
    }
}