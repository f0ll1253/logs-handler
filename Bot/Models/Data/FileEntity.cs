using Bot.Models.Abstractions;

namespace Bot.Models.Data {
	public class FileEntity : IEntity<string> {
		public string Id { get; set; } = null!;
		
		public string Name { get; set; } = null!;
		public string Extension { get; set; } = null!;
		public string Path { get; set; } = null!;

		// Foreign
		public FileTelegramInfo? TelegramInfo { get; set; }
		public long? TelegramInfoId { get; set; }

		//
		public static string GenerateId(string category, string service, string logs_name, string extension) {
			return System.IO.Path.Combine(category, service, $"{logs_name}.{extension}").Sha256();
		}

		public static FileEntity Create(string category, string service, string logs_name, string extension) {
			return new() {
				Id = FileEntity.GenerateId(category, service, logs_name, extension),
				Name = logs_name,
				Extension = extension,
				Path = System.IO.Path.Combine(category, service)
			};
		}
	}
}