using Autofac;
using Console.Models.Abstractions;
using Console.Views;
using Microsoft.Extensions.Configuration;
using Splat;
using Splat.Autofac;

namespace Console;

internal static class App
{
    private static readonly CancellationTokenSource _source = new();
    
    public static async Task Main()
    {
        InitializeServices();

        var root = Locator.Current.GetService<IRoot>()!;
        await root.Redirect<MainView>();
        await root.Start(Token);
    }

    public static CancellationToken Token => _source.Token;

    private static void InitializeServices()
    {
        var builder = new ContainerBuilder()
            .RegisterConfiguration()
            .RegisterViews();

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.SetLifetimeScope(builder.Build());
    }
    
    private static ContainerBuilder RegisterConfiguration(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        builder.RegisterInstance((IConfiguration) configBuilder.Build());

        return builder;
    }

    public static ContainerBuilder RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<Root>()
            .As<IRoot>()
            .SingleInstance();

        builder.RegisterType<MainView>();
        
        return builder;
    }
}