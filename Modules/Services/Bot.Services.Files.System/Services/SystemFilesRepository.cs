using Bot.Core.Models;
using Bot.Core.Models.Base;
using Bot.Core.Models.Files.Abstractions;
using Bot.Services.Files.System.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Services.Files.System.Services {
	public class SystemFilesRepository(DbContext context, IConfiguration config, ILogger<SystemFilesRepository> logger) : BaseReadOnlyRepository<SystemFile, string>(context, logger), IFilesRepository<SystemFile, string, SystemFilesArgs> {
		public Task<SystemFile?> GetAsync(IFileArgs args) {
			return context.Set<SystemFile>().FirstOrDefaultAsync(x => x.Name == args.Name && x.Extension == args.Extension && x.Service == args.Service);
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