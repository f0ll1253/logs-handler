using System.Text;
using Autofac;
using CG.Web.MegaApiClient;
using Core.Models.Configs;
using Core.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Splat;
using Splat.Autofac;
using TelegramBot.Commands;
using TelegramBot.Commands.Main;
using TelegramBot.Commands.Services;
using TelegramBot.Data;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using WTelegram;

using File = System.IO.File;

namespace TelegramBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await Program.InitializeServices();
        await Program.InitializeDatabase();
        Program.StartHookConsoleCommands();

        await Locator.Current.GetService<App>()!.Run();
    }

    #region Initialize Services

    private static Task InitializeServices()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Program.ConfigureLogging();

        var builder = new ContainerBuilder();

        builder.RegisterConfiguration();
        Log.Information("Success register configuration");

        builder.RegisterServices();
        Log.Information("Success register services");

        builder.RegisterViews();
        Log.Information("Success register views");

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

        AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => Log.Error(((Exception)args.ExceptionObject).ToString());
    }

    private static ContainerBuilder RegisterServices(this ContainerBuilder builder)
    {
        builder.Register<Client>(x => new Client(28352840, "e12baa8db750ab32ad22abab22b549c8"))
               .SingleInstance()
               .AsSelf();
        builder.RegisterType<App>()
               .SingleInstance()
               .AsSelf();
        builder.RegisterType<DataService>()
               .OnActivated(x =>
               {
                   string folder = x.Context.Resolve<IConfiguration>()["BaseFolder"]!;

                   Directory.CreateDirectory(folder);
                   Directory.CreateDirectory(Path.Combine(folder, "Logs"));
                   Directory.CreateDirectory(Path.Combine(folder, "Extracted"));
               })
               .SingleInstance()
               .AsSelf();

        builder.RegisterContext<AppDbContext>((_, options) => options.UseSqlite("Data Source=users.db"));

        builder.Register((MegaConfig config) =>
               {
                   var client = new MegaApiClient();

                   if (File.Exists("mega.json"))
                   {
                       client.LoginBySession("mega.json");
                   }
                   else
                   {
                       var session = client.LoginByCredentials(config);

                       File.WriteAllText("mega.json", JsonConvert.SerializeObject(session, Formatting.None));
                   }

                   return client;
               })
               .AutoActivate()
               .SingleInstance()
               .As<IMegaApiClient>();

        return builder;
    }

    private static ContainerBuilder RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<StartCommand>()
               .Named<ICommand>("/start");
        builder.RegisterType<InviteCommand>()
               .Named<ICommand>("/invite");

        // main
        builder.RegisterType<CookiesCommand>()
               .Named<ICommand>("Cookies")
               .Named<ICallbackCommand>("Cookies");
        builder.RegisterType<AccountsCommand>()
               .Named<ICommand>("Accounts")
               .Named<ICallbackCommand>("Accounts");

        // services
        builder.RegisterType<ServicesCommand>()
               .Named<ICommand>("Services");
        builder.RegisterType<TwitchServiceCommand>()
               .Named<ICommand>("Twitch")
               .Named<ICallbackCommand>("Twitch");
        builder.RegisterType<TelegramServiceCommand>()
               .Named<ICommand>("Telegram")
               .Named<ICallbackCommand>("Telegram");

        return builder;
    }

    #endregion

    private static async Task InitializeDatabase()
    {
        var context = Locator.Current.GetService<AppDbContext>()!;

        await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();

        if (!context.Users!.Any())
            context.Users!.Add(new User
            {
                id = 6572189077,
                IsApproved = true
            });

        await context.SaveChangesAsync();
    }

    private static void StartHookConsoleCommands()
    {
        async Task? HookCommands()
        {
            while (true)
            {
                string? str = Console.ReadLine();

                if (string.IsNullOrEmpty(str)) continue;

                if (str == "/create invitecode")
                {
                    var code = new InviteCode
                    {
                        Expire = DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(3600).Ticks
                    };
                    var context = Locator.Current.GetService<AppDbContext>()!;
                    await context.AddAsync(code);
                    await context.SaveChangesAsync();

                    Log.Information($"Generated new invite code: {code.Id} \t Code will expire: {DateTime.FromFileTimeUtc(code.Expire).ToShortTimeString()}");
                }
            }
        }

        Task.Run(HookCommands);
    }
}