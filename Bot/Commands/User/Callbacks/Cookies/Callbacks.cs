using Bot.Models.Abstractions;
using Bot.Parsers.Abstractions;
using Bot.Services;

namespace Bot.Commands.User.Callbacks.Cookies {
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_instagram")]
	public class InstagramCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".instagram.com"],
		"Instagram",

		parser,
		client,
		tasks,
		files
	);
	
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_twitter")]
	public class TwitterCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".x.com", ".twitter.com"],
		"Twitter",
		
		parser,
		client,
		tasks,
		files
	);
	
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_tiktok")]
	public class TiktokCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".tiktok.com"],
		"Tiktok",
		
		parser,
		client,
		tasks,
		files
	);
	
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_youtube")]
	public class YoutubeCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".gmail.com", ".google.com", ".youtube.com"],
		"Tiktok",
		
		parser,
		client,
		tasks,
		files
	);
	
	[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_linkedin")]
	public class LinkedInCallback([FromKeyedServices("parser_cookies")] IFileParser<IEnumerable<string>, string> parser, Client client, TasksManager tasks, FilesManager files) : Base.CookiesCallback(
		[".linkedin.com"],
		"LinkedIn",
		
		parser,
		client,
		tasks,
		files
	);
}