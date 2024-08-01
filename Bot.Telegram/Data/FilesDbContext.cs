using Bot.Services.Files.System.Models;
using Bot.Services.Files.System.Models.Abstractions;
using Bot.Services.Files.Telegram.Models;
using Bot.Services.Files.Telegram.Models.Abstractions;

using Microsoft.EntityFrameworkCore;

using DbContext = Bot.Core.Models.Overrides.DbContext;

namespace Bot.Telegram.Data {
	public class FilesDbContext(DbContextOptions options) : DbContext(options), ITelegramFilesDbContext, ISystemFilesDbContext {
		public DbSet<TelegramFile> TelegramFiles { get; set; }
		public DbSet<SystemFile> SystemFiles { get; set; }
	}
}