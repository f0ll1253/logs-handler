using Bot.Models.Abstractions;

namespace Bot.Models.Files {
	public class FileTelegramInfo : IEntity<long> {
		public long Id { get; set; }

		public long AccessHash { get; set; }
		public byte[] FileReference { get; set; } = [];
		public string Type { get; set; } = null!;

		// Foreign
		public FileEntity? File { get; set; }
		public string FileId { get; set; } = null!;
	}
}