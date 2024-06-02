using Bot.Data;
using Bot.Models;
using Bot.Services.Hosted;

using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

namespace Bot;

internal static class Program {
    public static void Main(string[] args) {
        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session"));
        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Extracted"));
        
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSerilog(config => config
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
        );

        // Instances
        builder.Services.AddActivatedSingleton(_ => 
            new Client(
                int.Parse(builder.Configuration["Bot:ApiId"]!), 
                builder.Configuration["Bot:ApiHash"]!, 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session", ".session")
            )
        );

        builder.Services.AddDbContext<UsersDbContext>(options => options
            .UseSqlite(builder.Configuration["ConnectionStrings:Users"])
        );

        // Services
        builder.Services.AddHostedService<Bootstrapper>();

        // Commands
        builder.Services.AddBot();

        var app = builder.Build();

        app.ConfigureLogging();
        app.InitializeContext<UsersDbContext>();

        app.Run();
    }
    
    public static void ConfigureLogging(this IHost app) {
        Helpers.Log = (lvl, str) => Log.Write((LogEventLevel)lvl, str);
    }

    public static void InitializeContext<TContext>(this IHost app) where TContext : DbContext {
        var context = app.Services.GetRequiredService<TContext>();
        
        context.Database.EnsureCreated();

        switch (context) {
            case UsersDbContext:
                app._InitializeUsers(context);
                break;
        }
    }

    private static void _InitializeUsers(this IHost app, DbContext context) {
        if (context.Set<ApplicationUser>().Any()) {
            return;
        }
        
        var config = app.Services.GetRequiredService<IConfiguration>();
            
        foreach (var id in config.GetRequiredSection("Bot:Admins").Get<List<long>>()) {
            context.Add(new ApplicationUser()
            {
                Id = id,
                Roles = ["Admin"]
            });
        }

        context.SaveChanges();
    }
}