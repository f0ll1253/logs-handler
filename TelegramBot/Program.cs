using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Splat;
using Splat.Autofac;
using Telegram.Bot;
using TelegramBot.Data;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.View;

namespace TelegramBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await Initialize();

        Task.Run(async () =>
        {
            while (true)
            {
                var str = Console.ReadLine();

                if (string.IsNullOrEmpty(str)) continue;

                if (str == "/create invitecode")
                {
                    var code = new InviteCode { Expire = DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(3600).Ticks };
                    var context = Locator.Current.GetService<AppDbContext>()!;
                    await context.AddAsync(code);
                    await context.SaveChangesAsync();
                    
                    Log.Information($"Generated new invite code: {code.Id} \t Code will expire: {DateTime.FromBinary(code.Expire).ToShortTimeString()}");
                }
            }
        });
        
        await Locator.Current.GetService<App>()!.Run();
    }
    
    private static Task Initialize()
    {
        ConfigureLogging();
        
        var builder = new ContainerBuilder()
            .RegisterConfiguration()
            .RegisterServices();

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.SetLifetimeScope(builder.Build());
        
        return Task.CompletedTask;
    }
    
    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt", LogEventLevel.Information)
#if DEBUG
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Information)
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

        var config = configBuilder.Build();
        
        builder.RegisterInstance<IConfiguration>(config);
        Log.Information("Success register configuration");
        
        return builder;
    }

    private static ContainerBuilder RegisterServices(this ContainerBuilder builder)
    {
        builder.Register(x =>
        {
            var config = x.Resolve<IConfiguration>();

            return new TelegramBotClient(config["Bot:API_KEY"]!);
        })
            .As<ITelegramBotClient>()
            .SingleInstance();
        
        builder.RegisterType<App>()
            .SingleInstance()
            .AsSelf();
        builder.RegisterContext<AppDbContext>((_, options) => options.UseSqlite("Data Source=users.db"));
        builder.RegisterType<StartView>()
            .As<CommandsView>()
            .SingleInstance();
        Log.Information("Success register services");
        
        return builder;
    }
}