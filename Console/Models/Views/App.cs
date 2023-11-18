using Autofac;
using Console.Models.Abstractions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Splat;
using Splat.Autofac;

namespace Console.Models.Views;

public static class App
{
    private static readonly CancellationTokenSource _source = new();
    
    public static CancellationToken Token => _source.Token;

    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt", LogEventLevel.Information)
            .MinimumLevel.Error()
            .Enrich.FromLogContext()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error(((Exception)args.ExceptionObject).ToString());
    }
    
    public static Task Initialize(Action<ContainerBuilder>? other = null)
    {
        var builder = new ContainerBuilder()
            .RegisterConfiguration();
        
        other?.Invoke(builder);

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.SetLifetimeScope(builder.Build());
        
        return Task.CompletedTask;
    }

    public static async Task Run(IView start)
    {
        var root = Locator.Current.GetService<IRoot>()!;
        root.PushRedirect(start);
        await root.Start(Token);
    }
    
    private static ContainerBuilder RegisterConfiguration(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        builder.RegisterInstance((IConfiguration) configBuilder.Build());

        return builder;
    }
}