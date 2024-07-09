using Bot.Core.Models.Files.Abstractions;

using TL;

namespace Bot.Services.Files.Telegram.Models {
	public class TelegramFile : IFile<long> {
		public long Id { get; set; }
		public string Name { get; set; }
		public string Extension { get; set; }

		public int Parts { get; set; }
		public string? Md5CheckSum { get; set; }

		public static implicit operator InputFileBase(TelegramFile file) => file.Md5CheckSum is null ?
				new InputFileBig {
					id = file.Id,
					name = file.Name,
					parts = file.Parts
				} :
				new InputFile {
					id = file.Id,
					parts = file.Parts,
					name = file.Name,
					md5_checksum = file.Md5CheckSum
				};
	}
}