using Bot.Models.Abstractions;
using Bot.Parsers.Abstractions;
using Bot.Services;

namespace Bot.Commands.User.Callbacks.Cookies {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_instagram")]
	public class InstagramCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".instagram.com"],
		"Cookies",
		"Instagram",
		"zip",
		"Parse Instagram cookies",
		"Instagram cookies not found",
		"#instagram #cookies",

		parser,
		client,
		tasks,
		files
	);
}