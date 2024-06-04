using Bot.Data;
using Bot.Models.Users;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace Bot.Services.Hosted {
	public class DatabaseInitializationService(
			IServiceProvider provider,
			IConfiguration configuration
		) : IHostedService {
		public Task StartAsync(CancellationToken cancellationToken) {
			_InitializeContext<UsersDbContext>();
			_InitializeContext<DataDbContext>();

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			return Task.CompletedTask;
		}
		
		private void _InitializeContext<TContext>() where TContext : DbContext {
			var context = provider.GetRequiredService<TContext>();

			context.Database.EnsureCreated();

			switch (context) {
				case UsersDbContext:
					_InitializeUsers(context);
					break;
			}
		}

		private void _InitializeUsers(DbContext context) {
			if (context.Set<ApplicationUser>().Any()) {
				return;
			}
			foreach (var id in configuration.GetRequiredSection("Bot:Admins").Get<List<long>>()) {
				context.Add(
					new ApplicationUser {
						Id = id,
						Roles = ["Admin"]
					}
				);
			}

			context.SaveChanges();
		}
	}
}