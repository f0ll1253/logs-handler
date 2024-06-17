using System.Reflection;

using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

using WTelegram;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSlimMessageBus(
	config => {
		config.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
		
		config.WithProviderMemory()
			  .AutoDeclareFrom(Assembly.GetExecutingAssembly());
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