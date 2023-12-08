using Autofac;
using Console.Models.Abstractions;
using Core.Models.Extensions;
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
        AppDomain.CurrentDomain.ProcessExit += (sender, args) => Locator.Current.GetService<ILifetimeScope>()!.Dispose();
        
        return Task.CompletedTask;
    }

    public static async Task Run(IViewDefault start)
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

        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error(((Exception) args.ExceptionObject).ToString());
    }
}