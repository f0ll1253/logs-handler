using Autofac;
using Autofac.Core;
using Console.Models.Abstractions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Splat;
using Splat.Autofac;

namespace Console.Models.Views;

public static class App
{
    public static CancellationTokenSource Source => new ();
    
    public static Task Initialize(Action<ContainerBuilder>? other = null)
    {
        ConfigureLogging();
        
        // services
        var builder = new ContainerBuilder()
            .RegisterConfiguration();
        
        other?.Invoke(builder);

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.SetLifetimeScope(builder.Build());
        
        //
        AppDomain.CurrentDomain.ProcessExit += async (sender, args) => await Locator.Current.GetService<ILifetimeScope>()!.DisposeAsync();
        
        return Task.CompletedTask;
    }

    public static async Task Run(IView start)
    {
        var root = Locator.Current.GetService<IRoot>()!;
        root.PushRedirect(start);
        await root.Start(Source.Token);
    }
    
    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt", LogEventLevel.Information)
#if DEBUG
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Error)
#endif
            .Enrich.FromLogContext()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error(((Exception)args.ExceptionObject).ToString());
    }
    
    private static ContainerBuilder RegisterConfiguration(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var build = configBuilder.Build();
        
        builder.RegisterInstance(build);
        builder.RegisterInstance(build.GetRequiredSection("Web3").Get<Web3Config>()!);
        builder.RegisterInstance(build.GetRequiredSection("ParsingConfig").Get<ParsingConfig>()!);

        return builder;
    }
}