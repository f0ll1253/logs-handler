using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.System.Models;
using Bot.Services.Files.System.Services;
using Bot.Services.Files.Telegram.Models;
using Bot.Services.Files.Telegram.Services;

namespace Bot.Telegram.Services {
	public class FilesManager(SystemFilesRepository files_system, TelegramFilesRepository files_telegram) {
		public async Task<(SystemFile system_file, TelegramFile telegram_file)> CreateOrGetAsync(Func<Stream, Task> on_write, SystemFilesArgs args_system, TelegramFilesArgs args_telegram) {
			var telegram = await files_telegram.GetAsync(args_telegram);
			var system = await files_system.GetAsync(args_system);

			if (telegram is null || system is null) {
				using (var memory = new MemoryStream()) {
					await on_write.Invoke(memory);
				
					// Save system
					system ??= await files_system.CreateAsync(memory, args_system, false);
				
					await files_system.AddAsync(system);

					// Save telegram
					telegram ??= await files_telegram.CreateAsync(memory, args_telegram, false);

					await files_telegram.AddAsync(telegram);
				}
			}

			return (system, telegram);
		}
	}
}