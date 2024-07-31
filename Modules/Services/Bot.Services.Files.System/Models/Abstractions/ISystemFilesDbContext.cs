using Microsoft.EntityFrameworkCore;

namespace Bot.Services.Files.System.Models.Abstractions {
	public interface ISystemFilesDbContext {
		DbSet<SystemFile> SystemFiles { get; set; }
	}
}