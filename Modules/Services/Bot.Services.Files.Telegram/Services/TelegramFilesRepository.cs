using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.Telegram.Models;
using Bot.Services.Files.Telegram.Models.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TL;

using WTelegram;

namespace Bot.Services.Files.Telegram.Services {
	public class TelegramFilesRepository(Client client, ITelegramFilesDbContext context, ILogger<TelegramFilesRepository> logger) : IFilesRepository<TelegramFile, long, TelegramFilesArgs> {
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
		
		public Task<TelegramFile?> GetAsync(IFileArgs args) {
			return context.TelegramFiles.FirstOrDefaultAsync(x => x.Name == args.Name && x.Extension == args.Extension && x.Service == args.Service);
		}

		public async Task<TelegramFile> CreateAsync(Stream stream, TelegramFilesArgs args, bool dispose_stream = true) {

			#region Upload file

			stream.Seek(0, SeekOrigin.Begin);

			var info = await client.UploadFileAsync(stream, $"{args.Name}.{args.Extension}");

			if (dispose_stream) {
				await stream.DisposeAsync();
			}

			#endregion

			var file = (TelegramFile)info;

			file.Extension = args.Extension;
			file.MimeType = args.MimeType;
			file.Service = args.Service;

			return file;
		}
	}

	public record TelegramFilesArgs(string Name, string Extension, string Service, string MimeType) : IFileArgs {
		public string Name { get; set; } = Name;
		public string Extension { get; set; } = Extension;
		public string Service { get; set; } = Service;
		public string MimeType { get; set; } = MimeType;
	}
}