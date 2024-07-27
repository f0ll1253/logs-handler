using System.Reflection;

using Bot.Core.Messages.WTelegram;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Proxies.Data;
using Bot.Services.Proxies.Models;
using Bot.Services.Proxies.Services;
using Bot.Telegram.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers;

using Microsoft.EntityFrameworkCore;

using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

using TL;

using WTelegram;

var builder = Host.CreateApplicationBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.json");
#endif

// Message Bus
builder.Services.AddSlimMessageBus(
	config => {
		config.AddServicesFromAssembly(Assembly.GetExecutingAssembly());

		config.WithProviderMemory()
			  .AutoDeclareFrom(Assembly.GetExecutingAssembly());

		config.Handle<UpdateHandlerRequest, UpdateHandlerResponse>(
			builder => {
				builder.Path(
					nameof(UpdateNewMessage),
					x => { x.WithHandler<UpdateNewMessageHandler>(); }
				);
			}
		);

		config.Handle<UpdateHandlerRequest, UpdateHandlerResponse>(
			builder => {
				builder.Path(
					nameof(UpdateBotCallbackQuery),
					x => { x.WithHandler<UpdateBotCallbackQueryHandler>(); }
				);
			}
		);
	}
);

// WTelegram
builder.Services.AddSingleton<Client>(
	_ => new(
		int.Parse(builder.Configuration["Bot:ApiId"]!),
		builder.Configuration["Bot:ApiHash"],
		".session"
	)
);

// Services
builder.Services.AddHostedService<Bootstrapper>();

// Proxies
builder.Services.AddDbContext<ProxiesDbContext>(x => x.UseInMemoryDatabase("Proxies"));
builder.Services.AddSingleton<Proxies>();

// Discord
builder.Services.AddSingleton<IChecker<Bot.Services.Discord.Models.User>, Bot.Services.Discord.Checker>(x => new(x.GetRequiredService<ILoggerFactory>().CreateLogger<Bot.Services.Discord.Checker>()));
builder.Services.AddSingleton<IParserStream<Bot.Services.Discord.Models.User>, Bot.Services.Discord.Parser>();

// Projects inject
builder.Services.AddBotTelegram();

var host = builder.Build();

// Initialize proxies
using (var scope = host.Services.CreateScope()) {
	var context = scope.ServiceProvider.GetRequiredService<ProxiesDbContext>();
	var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
	var section = config.GetSection("Proxies");

	context.AddRange(section.Get<List<string>>()!.Select(x => (Proxy)x));
	context.SaveChanges();
}

host.Run();