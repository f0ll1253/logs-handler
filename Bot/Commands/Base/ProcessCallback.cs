using Bot.Commands.Markups;
using Bot.Models.Abstractions;
using Bot.Models.Data;
using Bot.Services;

namespace Bot.Commands.Base {
	public abstract class ProcessCallback(
		string category,
		string service,
		string file_type,
		string task_name,
		string not_found_message,
		string hashtags,
		
		// Services
		Client client, 
		TasksManager tasks, 
		FilesManager files
		) : ICommand<UpdateBotCallbackQuery> {
		public string Category { get; } = category;
		public string Service { get; } = service;
		public string FileType { get; } = file_type;
		public string TaskName { get; } = task_name;
		public string NotFoundMessage { get; } = not_found_message;
		public string Hashtags { get; } = hashtags;
		
		public async Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			var data = update.data.Utf8().Split(':');

			await tasks.RegisterTask(_SendDataAsync(user, data[1]), user.id, TaskName);
		}

		private protected async Task _SendDataAsync(TL.User user, string logs_name) {
			if (await files.TryGetFileAsync(Category, Service, logs_name, FileType) is not { } entity) {
				await client.Messages_SendMessage(
					user,
					$"Task '{TaskName}' was started",
					Random.Shared.NextInt64(),
					reply_markup: Markup_General.Dispose
				);
				
				entity = await _ParseAsync(logs_name);

				if (entity is null) {
					await client.Messages_SendMessage(
						user,
						NotFoundMessage,
						Random.Shared.NextInt64(),
						reply_markup: Markup_General.Dispose
					);

					return;
				}
			}

			await files.SendFileAsync(entity, user, Hashtags);
		}

		private protected abstract Task<FileEntity?> _ParseAsync(string logs_name);
	}
}