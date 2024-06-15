using System.Reflection;

using Bot.Database;
using Bot.Models;
using Bot.Payments.CryptoBot.Extensions;
using Bot.Payments.CryptoBot.Services.Hosted;
using Bot.Services.Hosted;

using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

namespace Bot {
	internal static class Program {
		public static void Main(string[] args) {
			Directory.CreateDirectory(Constants.Directory_Session);
			Directory.CreateDirectory(Constants.Directory_Extracted);
			Directory.CreateDirectory(Constants.Directory_Downloaded);

#if Bot_UseEndpoints
			var builder = WebApplication.CreateBuilder(args);
#else
			var builder = Host.CreateApplicationBuilder(args);
#endif

			// Logging
			builder.Services.AddSerilog(
				config => config
						  .MinimumLevel
						  .Verbose()
						  .Enrich
						  .FromLogContext()
						  .WriteTo
						  .Console()
			);

			// Configuration
			builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

			// Endpoints
			builder.Services
				   .AddControllers()
				   .AddNewtonsoftJson();
			
			// Instances
			builder.Services.AddActivatedSingleton(
				_ =>
						new Client(
							int.Parse(builder.Configuration["Bot:ApiId"]!),
							builder.Configuration["Bot:ApiHash"]!,
							Path.Combine(Constants.Directory_Session, ".session")
						)
			);

			builder.Services.AddDbContext<UsersDbContext>(
				options => options
						.UseSqlite(builder.Configuration["ConnectionStrings:Users"])
			);

			builder.Services.AddDbContext<DataDbContext>(
				options => options
						.UseSqlite(builder.Configuration["ConnectionStrings:Data"])
			);

			builder.Services.AddDbContext<PaymentsDbContext>(
				options => options
						.UseSqlite(builder.Configuration["ConnectionStrings:Payments"])
			);

			// Services
			builder.Services.AddHostedService<BotInitializationService>();
			builder.Services.AddHostedService<DatabaseInitializationService>();

			// Injectio
			builder.Services.AddBot();
			builder.Services.AddBotParsers();
			builder.Services.AddBotCheckers();
			builder.Services.AddBotServices();

			builder.Services.AddBotPaymentsCryptoBot();
			builder.Services.AddBotPaymentsCryptoBotDependencies();

			var app = builder.Build();

			app.ConfigureLogging();

#if Bot_UseEndpoints
			app.MapControllers();
#endif

			app.Run();
		}

		public static void ConfigureLogging(this IHost app) {
			Helpers.Log = (lvl, str) => Log.Write((LogEventLevel)lvl, str);
		}
	}
}