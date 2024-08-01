using Bot.Core.Models.Files.Abstractions;

using TL;

namespace Bot.Services.Files.Telegram.Models {
	public class TelegramFile : IFile<long> {
		public long Id { get; set; }
		public string Name { get; set; }
		public string Extension { get; set; }
		public string Service { get; set; }
		public string MimeType { get; set; }
		public bool IsBig { get; set; }

		public int Parts { get; set; }
		public string? Md5CheckSum { get; set; }

		// From
		public static implicit operator InputFileBase(TelegramFile file) => file.IsBig ?
				new InputFileBig {
					id = file.Id,
					name = $"{file.Name}.{file.Extension}",
					parts = file.Parts
				} :
				new InputFile {
					id = file.Id,
					parts = file.Parts,
					name = $"{file.Name}.{file.Extension}",
					md5_checksum = file.Md5CheckSum
				};

		public static implicit operator InputMedia(TelegramFile file) => file.MimeType switch {
			_ => new InputMediaUploadedDocument(file, file.MimeType)
		};
		
		// To
		public static implicit operator TelegramFile(InputFileBase info) {
			var telegram_file = new TelegramFile();

			if (info is InputFileBig big) { // is big (more than 20 mb)
				telegram_file.Id = big.id;

				var temp = big.name.Split('.');
				telegram_file.Name = temp[0];
				telegram_file.Extension = temp[1];

				telegram_file.Parts = big.Parts;
			}
			else if (info is InputFile file) {
				telegram_file.Id = file.id;

				var temp = file.Name.Split('.');
				telegram_file.Name = temp[0];
				telegram_file.Extension = temp[1];
				
				telegram_file.Parts = file.Parts;
				telegram_file.Md5CheckSum = file.md5_checksum;
			}

			telegram_file.IsBig = info is InputFileBig;
			
			return telegram_file;
		}
	}
}