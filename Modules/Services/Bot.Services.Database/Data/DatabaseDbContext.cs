using Bot.Services.Database.Models;

using Microsoft.EntityFrameworkCore;

using DbContext = Bot.Core.Models.Overrides.DbContext;

namespace Bot.Services.Database.Data {
	public class DatabaseDbContext : DbContext {
		public DatabaseDbContext() { }
		public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options) : base(options) { }
		
		public DbSet<Credential> Credentials { get; set; }
	}
}