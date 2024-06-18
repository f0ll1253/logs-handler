using System.Reflection;

using Bot.Core.Messages.WTelegram;
using Bot.Telegram.WTelegram.UpdateHandlers;

using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

using TL;

using WTelegram;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSlimMessageBus(
	config => {
		config.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
		
		config.WithProviderMemory()
			  .AutoDeclareFrom(Assembly.GetExecutingAssembly());

		config.Handle<UpdateHandlerRequest, UpdateHandlerResponse>(
			builder => {
				builder.Path(
					nameof(UpdateNewMessage),
					x => {
						x.WithHandler<UpdateNewMessageHandler>();
					}
				);
			}
		);
		
		config.Handle<UpdateHandlerRequest, UpdateHandlerResponse>(
			builder => {
				builder.Path(
					nameof(UpdateBotCallbackQuery),
					x => {
						x.WithHandler<UpdateBotCallbackQueryHandler>();
					}
				);
			}
		);
	}
);

builder.Services.AddSingleton<Client>(
	_ => new(
		int.Parse(builder.Configuration["Bot:ApiId"]!),
		builder.Configuration["Bot:ApiHash"],
		".session"
	)
);

// Services
builder.Services.AddHostedService<Bot.Telegram.WTelegram.Bootstrapper>();

// Services
builder.Services.AddBotTelegram();

var host = builder.Build();

host.Run();