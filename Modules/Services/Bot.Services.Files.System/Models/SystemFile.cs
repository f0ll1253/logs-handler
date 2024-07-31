using Bot.Core.Models.Files.Abstractions;

namespace Bot.Services.Files.System.Models {
	public class SystemFile : IFile<string> {
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public string Extension { get; set; }
		public string Service { get; set; }
	}
}