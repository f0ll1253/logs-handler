namespace Bot.Core.Models.Files.Abstractions {
	public interface IFile<TKey> {
		TKey Id { get; set; }
		string Name { get; set; }
		string Extension { get; set; }
	}
}