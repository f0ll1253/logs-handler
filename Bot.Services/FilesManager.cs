using Bot.Database;
using Bot.Models.Files;

using Injectio.Attributes;

using TL;

using WTelegram;

using Constants = Bot.Models.Constants;

namespace Bot.Services {
	[RegisterScoped]
	public class FilesManager(Client client, DataDbContext context) {
		public Task<FileEntity?> TryGetFileAsync(string id) {
			return context.FindAsync<FileEntity>(id).AsTask();
		}

		public Task<FileEntity?> TryGetFileAsync(string category, string service, string logs_name, string extension) {
			return TryGetFileAsync(FileEntity.GenerateId(category, service, logs_name, extension));
		}

		/// <remarks>
		///     File path "<see cref="Models.Constants.Directory_Data" />/<paramref name="category" />/<paramref name="service" />/Name.
		///     <paramref name="extension" />"
		/// </remarks>
		/// <param name="category">Type of file like 'Cookies', 'Accounts' etc.</param>
		/// <param name="service">Name of service 'Instagram', 'Discord' etc. (without '.')</param>
		public async Task<FileEntity> SaveFileAsync(Action<Stream> saveTo, string category, string service, string logs_name, string extension) {
			var file = FileEntity.Create(category, service, logs_name, extension);

			// Return exist entity
			if (await TryGetFileAsync(file.Id) is { } entity) {
				return entity;
			}

			// Save
			var info = new FileInfo(Path.Combine(Constants.Directory_Data, file.Path, $"{file.Name}.{file.Extension}"));

			if (!info.Directory.Exists) {
				info.Directory.Create();
			}

			using (var writer = info.Create()) {
				saveTo.Invoke(writer);
			}

			await context.AddAsync(file);
			await context.SaveChangesAsync();

			return file;
		}

		public async Task<FileTelegramInfo> SendFileAsync(FileEntity entity, User user, string caption) {
			FileTelegramInfo info;

			if (entity.TelegramInfoId is { } id) {
				info = (await context.FindAsync<FileTelegramInfo>(id))!;
				var uploaded = (InputMedia)new InputDocument {
					id = info.Id,
					access_hash = info.AccessHash,
					file_reference = info.FileReference
				};

				await client.Messages_SendMedia(
					user,
					uploaded,
					caption,
					Random.Shared.NextInt64()
				);

				return info;
			}

			var input_file = await client.UploadFileAsync(Path.Combine(Constants.Directory_Data, entity.Path, $"{entity.Name}.{entity.Extension}"));
			var input = new InputMediaUploadedDocument(
				input_file,
				entity.Extension switch {
					"zip" => "application/zip",
					"txt" => "text/plain",
					_     => "unknown"
				}
			);

			var message = await client.Messages_SendMedia(
				user,
				input,
				caption,
				Random.Shared.NextInt64()
			);

			var document = (Document)((MessageMediaDocument)((Message)((UpdateNewMessage)message
																						 .UpdateList
																						 .First(x => x is UpdateNewMessage))
							.message)
						.media)
					.document;

			info = new() {
				Id = document.id,
				AccessHash = document.access_hash,
				FileReference = document.file_reference,
				Type = document.mime_type,
				FileId = entity.Id
			};

			entity.TelegramInfoId = info.Id;

			context.Update(entity);
			await context.AddAsync(info);
			await context.SaveChangesAsync();

			return info;
		}
	}
}