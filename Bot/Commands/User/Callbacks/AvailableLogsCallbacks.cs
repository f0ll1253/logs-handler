using Bot.Commands.Base;
using Bot.Models.Abstractions;

namespace Bot.Commands.User.Callbacks;

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_instagram")]
public class InstagramLogsCallback(Client client) : AvailableLogsCallback("instagram", "instagram_process", client);

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_twitter")]
public class TwitterLogsCallback(Client client) : AvailableLogsCallback("twitter", "twitter_process", client);

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_twitch")]
public class TwitchLogsCallback(Client client) : AvailableLogsCallback("twitch", "twitch_process", client);

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_telegram")]
public class TelegramLogsCallback(Client client) : AvailableLogsCallback("telegram", "telegram_process", client);

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_tiktok")]
public class TiktokLogsCallback(Client client) : AvailableLogsCallback("tiktok", "tiktok_process", client);

[RegisterTransient<ICommand<UpdateBotCallbackQuery>>(ServiceKey = "user_youtube")]
public class YouTubeLogsCallback(Client client) : AvailableLogsCallback("youtube", "youtube_process", client);