using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.Telegram.Models;
using Bot.Services.Files.Telegram.Models.Abstractions;

using Injectio.Attributes;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TL;

using WTelegram;

namespace Bot.Services.Files.Telegram.Services {
	[RegisterTransient<IFilesRepository<TelegramFile, long>>]
	public class TelegramFilesRepository(Client client, ITelegramFilesDbContext context, ILogger<TelegramFilesRepository> logger) : IFilesRepository<TelegramFile, long> {
		public async Task<bool> AddAsync(TelegramFile obj) {
			if (context is not DbContext db) {
				throw new ArgumentException("ITelegramFilesDbContext is not DbContext", nameof(context));
			}
			
			context.TelegramFiles.Add(obj);

			try {
				await db.SaveChangesAsync();
			} catch (DbUpdateException e) {
				db.ChangeTracker.Clear();
				logger.LogError(e, "Error while saving file {id} '{name}.{extension}'", obj.Id, obj.Name, obj.Extension);
				return false;
			}
			
			logger.LogInformation("Saved new file {id} '{name}.{extension}'", obj.Id, obj.Name, obj.Extension);

			return true;
		}

		public Task<TelegramFile?> GetAsync(long key) {
			return context.TelegramFiles.FindAsync(key).AsTask();
		}

		public async Task<TelegramFile> CreateAsync(Stream stream, string name, string extension, bool dispose_stream = true) {

			#region Upload file

			stream.Seek(0, SeekOrigin.Begin);

			var info = await client.UploadFileAsync(stream, $"{name}.{extension}");

			if (dispose_stream) {
				await stream.DisposeAsync();
			}

			#endregion

			var telegram_file = new TelegramFile {
				Extension = extension
			};

			if (stream.Length >= 10 * 1024 * 1024 && info is InputFileBig big) { // is big (more than 20 mb)
				telegram_file.Id = big.id;
				telegram_file.Name = big.name;

				telegram_file.Parts = big.Parts;
			}
			else if (info is InputFile file) {
				telegram_file.Id = file.id;
				telegram_file.Name = file.Name;

				telegram_file.Md5CheckSum = file.md5_checksum;
			}

			return telegram_file;
		}
	}
}