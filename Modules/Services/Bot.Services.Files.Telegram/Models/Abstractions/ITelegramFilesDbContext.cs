using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Files.Telegram.Models.Abstractions {
	public interface ITelegramFilesDbContext {
		DbSet<TelegramFile> TelegramFiles { get; set; }
	}
}