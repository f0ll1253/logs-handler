namespace Bot.Core.Models.Files.Abstractions {
	public interface IFileArgs {
		string Name { get; set; }
		string Extension { get; set; }
		string Service { get; set; }
	}
}