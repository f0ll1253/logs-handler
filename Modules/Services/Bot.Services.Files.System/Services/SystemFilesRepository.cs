using Bot.Core.Models;
using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.System.Models;
using Bot.Services.Files.System.Models.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Services.Files.System.Services {
	public class SystemFilesRepository(ISystemFilesDbContext context, IConfiguration config, ILogger<SystemFilesRepository> logger) : IFilesRepository<SystemFile, string, SystemFilesArgs> {
		public async Task<bool> AddAsync(SystemFile entity) {
			if (context is not DbContext db) {
				throw new ArgumentException("ISystemFilesDbContext is not DbContext", nameof(context));
			}
			
			context.SystemFiles.Add(entity);

			try {
				await db.SaveChangesAsync();
			} catch (DbUpdateException e) {
				db.ChangeTracker.Clear();
				logger.LogError(e, "Error while saving file {id} '{name}.{extension}'", entity.Id, entity.Name, entity.Extension);
				return false;
			}
			
			logger.LogInformation("Saved new file {id} '{name}.{extension}'", entity.Id, entity.Name, entity.Extension);

			return true;
		}

		public Task<SystemFile?> GetAsync(string key) {
			return context.SystemFiles.FindAsync(key).AsTask();
		}

		public Task<SystemFile?> GetAsync(IFileArgs args) {
			return context.SystemFiles.FirstOrDefaultAsync(x => x.Name == args.Name && x.Extension == args.Extension && x.Service == args.Service);
		}

		public async Task<SystemFile> CreateAsync(Stream stream, SystemFilesArgs args, bool dispose_stream = true) {
			
			#region Save File

			stream.Seek(0, SeekOrigin.Begin);
            
			await using (var file = File.OpenWrite(Path.Combine(config.GetPath(args.Service), $"{args.Name}.{args.Extension}"))) {
				await stream.CopyToAsync(file);
			}

			if (dispose_stream) {
				await stream.DisposeAsync();
			}

			#endregion

			return new() {
				Name = args.Name,
				Extension = args.Extension,
				Service = args.Service
			};
		}
	}

	public record SystemFilesArgs(string Name, string Extension, string Service) : IFileArgs {
		public string Name { get; set; } = Name;
		public string Extension { get; set; } = Extension;
		public string Service { get; set; } = Service;
	}
}