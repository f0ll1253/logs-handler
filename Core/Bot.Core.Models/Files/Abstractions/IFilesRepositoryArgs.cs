namespace Bot.Core.Models.Files.Abstractions {
	public interface IFilesRepositoryArgs {
		public string Name { get; set; }
		public string Extension { get; set; }
	}
}