using Bot.Models.Users;

using Microsoft.EntityFrameworkCore;

using Task = Bot.Models.Users.Task;

namespace Bot.Data {
	public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options) {
		public DbSet<ApplicationUser> Users { get; set; }
		public DbSet<Task> Tasks { get; set; }

		protected override void OnModelCreating(ModelBuilder builder) {
			builder.Entity<ApplicationUser>(
				entity => {
					entity.HasMany(x => x.Tasks)
						  .WithOne(x => x.User)
						  .HasPrincipalKey(x => x.Id)
						  .HasForeignKey(x => x.UserId);
				}
			);

			base.OnModelCreating(builder);
		}
	}
}