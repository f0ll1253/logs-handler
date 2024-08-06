using System.Reflection;

using Bot.Core.Messages.WTelegram;
using Bot.Core.Models.Checkers.Abstractions;
using Bot.Core.Models.Parsers.Abstractions;
using Bot.Services.Files.System.Models.Abstractions;
using Bot.Services.Files.System.Services;
using Bot.Services.Files.Telegram.Models.Abstractions;
using Bot.Services.Files.Telegram.Services;
using Bot.Services.Proxies.Data;
using Bot.Services.Proxies.Services;
using Bot.Telegram.Data;
using Bot.Telegram.Services;
using Bot.Telegram.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers;

using Hangfire;

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

// Hangfire
builder.Services.AddHangfire(
	config => {
		config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
		config.UseColouredConsoleLogProvider();
		config.UseSimpleAssemblyNameTypeSerializer();
		config.UseRecommendedSerializerSettings();
		config.UseInMemoryStorage();
	}
);

builder.Services.AddHangfireServer();

// Services
builder.Services.AddHostedService<Bootstrapper>();

// Proxies
builder.Services.AddDbContext<ProxiesDbContext>(x => x.UseInMemoryDatabase("Proxies"));
builder.Services.AddSingleton<Proxies>();

// Discord
builder.Services.AddSingleton<IChecker<Bot.Services.Discord.Models.User>, Bot.Services.Discord.Checker>(x => new(x.GetRequiredService<ILoggerFactory>().CreateLogger<Bot.Services.Discord.Checker>()));
builder.Services.AddSingleton<IParserStream<Bot.Services.Discord.Models.User>, Bot.Services.Discord.Parser>();

// Twitch
builder.Services.AddSingleton<IParserStream<Bot.Services.Twitch.Models.User>, Bot.Services.Twitch.Parser>();

// Files
builder.Services.AddDbContext<FilesDbContext>(x => x.UseInMemoryDatabase("Files"));
builder.Services.AddScoped<ITelegramFilesDbContext>(x => x.GetRequiredService<FilesDbContext>());
builder.Services.AddScoped<ISystemFilesDbContext>(x => x.GetRequiredService<FilesDbContext>());

builder.Services.AddSingleton<SystemFilesRepository>();
builder.Services.AddSingleton<TelegramFilesRepository>();

builder.Services.AddSingleton<FilesManager>();

// Projects inject
builder.Services.AddBotTelegram();

var host = builder.Build();

host.Run();