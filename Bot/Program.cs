using System.Reflection;

using Bot.Data;
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

			var builder = Host.CreateApplicationBuilder(args);

			builder.Services.AddSerilog(
				config => config
						  .MinimumLevel
						  .Verbose()
						  .Enrich
						  .FromLogContext()
						  .WriteTo
						  .Console()
			);

			builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

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

			// Services
			builder.Services.AddHostedService<BotInitializationService>();
			builder.Services.AddHostedService<DatabaseInitializationService>();

			// Injectio
			builder.Services.AddBot();
			builder.Services.AddBotParsers();
			builder.Services.AddBotCheckers();

			var app = builder.Build();

			app.ConfigureLogging();

			app.Run();
		}

		public static void ConfigureLogging(this IHost app) {
			Helpers.Log = (lvl, str) => Log.Write((LogEventLevel)lvl, str);
		}
	}
}