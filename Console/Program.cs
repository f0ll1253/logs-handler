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
        
        await LoadProxies();
        
        await App.Run(Locator.Current.GetService<StartView>()!);
    }

    private static async Task LoadProxies()
    {
        System.Console.Write("Path to proxies: ");
        var proxiespath =  System.Console.ReadLine()?.Replace("\"", "");

        if (!File.Exists(proxiespath)) throw new Exception("Proxies file doesn't exists");

        var proxypool = Locator.Current.GetService<ProxyPool>()!;
        var proxies = new List<Proxy>();
        using var reader = new StreamReader(proxiespath);

        while (!reader.EndOfStream)
        {
            var args = (await reader.ReadLineAsync())?.Replace('@', ':').Split(':');
            
            if (args is not {Length:>=2}) continue;

            switch (args.Length)
            {
                case 2:
                    proxies.Add(new Proxy(args[0], int.Parse(args[1])));
                    break;
                case 4:
                    proxies.Add(new Proxy(args[2], int.Parse(args[3]), args[0], args[1]));
                    break;
            }
        }

        proxypool.AddRange(proxies);
    }

    private static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<Settings>()
            .SingleInstance();
        builder.RegisterType<ProxyPool>()
            .SingleInstance();
        
        builder.RegisterType<DataService>();
        builder.RegisterType<DiscordChecker>();
        builder.RegisterType<IGVChecker>();
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