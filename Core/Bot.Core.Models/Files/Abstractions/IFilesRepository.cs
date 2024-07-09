using Bot.Core.Models.Abstractions;

namespace Bot.Core.Models.Files.Abstractions {
	public interface IFilesRepository<TFile, in TKey> : IRepository<TFile, TKey> where TFile : class, IFile<TKey> {
		Task<TFile> CreateAsync(Stream stream, string name, string extension, bool dispose_stream = true);
	}
}