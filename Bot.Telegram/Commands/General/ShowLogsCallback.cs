using System.Text;

using Bot.Core.Models.Commands.Abstractions;

using Injectio.Attributes;

using TL;

using WTelegram;

namespace Bot.Telegram.Commands.General {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = Keys.ShowLogsCallback)]
	public class ShowLogsCallback(Client client, IConfiguration config) : ICommand<UpdateBotCallbackQuery> {
		private const byte n_Action = 1;
		private const byte n_Page = 2;
		private const byte n_Back = 3;

		public static byte[] CreateData(byte action, byte back) => [Keys.ShowLogsCallback, action, 0, back];
		
		public async Task ExecuteAsync(UpdateBotCallbackQuery update, TL.User user) {
			#region Arguments
			
			var action = update.data[n_Action];
			var page = update.data[n_Page];
			var back = update.data[n_Back];
			
			#endregion

			var buttons = new List<KeyboardButtonCallback>();

			if (new DirectoryInfo(Path.Combine(config["Files:Root"], "Extracted")).GetDirectories().OrderByDescending(x => x.CreationTimeUtc).ToArray() is not {Length: > 1} directories) {
				await client.Messages_GetBotCallbackAnswer(
					user,
					update.msg_id,
					"No available logs"u8.ToArray()
				);
				
				return;
			}

			foreach (var directory in directories.Take(new Range(page * 5, page * 5 + 5))) {
				buttons.Add(new() {
					text = directory.Name,
					data = [action, ..Encoding.UTF8.GetBytes(directory.Name)]
				});
			}

			await client.Messages_EditMessage(
				user,
				update.msg_id,
				$$"""
				🗂 Available logs
				📜 Page: {{page + 1}}
				""",
				reply_markup: new ReplyInlineMarkup {
					rows = [
						..buttons.Select(x => new KeyboardButtonRow {
							buttons = [x]
						}),
						_CreateNavigation(
							action, 
							page,
							back,
							directories.Length
						),
						back > 0 ? new KeyboardButtonRow() {
							buttons = [
								new KeyboardButtonCallback {
									text = "Back",
									data = [back]
								}
							]
						} : null
					]
				}
			);
		}
		
		// Private
		private static KeyboardButtonRow? _CreateNavigation(byte action, byte page, byte back, int found_directories) {
			var row = new KeyboardButtonRow {
				buttons = [
					page > 0 ? new KeyboardButtonCallback {
						text = "\u25c0\ufe0f",
						data = [Keys.ShowLogsCallback, action, (byte)(page - 1), back]
					} : null, 
					found_directories - (page + 1) * 5 > 0 ? new KeyboardButtonCallback {
						text = "\u25b6\ufe0f",
						data = [Keys.ShowLogsCallback, action, (byte)(page + 1), back]
					} : null
				]
			};

			return row.buttons.Count(x => x is { }) == 0 ? null : row;
		}
	}
}