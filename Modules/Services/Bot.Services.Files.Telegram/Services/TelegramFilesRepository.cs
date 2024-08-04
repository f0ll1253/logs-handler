using Bot.Core.Models.Base;
using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.Telegram.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using WTelegram;

namespace Bot.Services.Files.Telegram.Services {
	public class TelegramFilesRepository(Client client, DbContext context, ILogger<TelegramFilesRepository> logger) : BaseReadOnlyRepository<TelegramFile, long>(context, logger), IFilesRepository<TelegramFile, long, TelegramFilesArgs> {
		public Task<TelegramFile?> GetAsync(IFileArgs args) {
			return context.Set<TelegramFile>().FirstOrDefaultAsync(x => x.Name == args.Name && x.Extension == args.Extension && x.Service == args.Service);
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