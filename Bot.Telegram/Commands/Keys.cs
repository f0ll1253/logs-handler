using TL;

namespace Bot.Telegram.Commands {
	public static class Keys {
		public const string Start = "/start";
		public const byte StartCallback = 0;
		
		public const byte ShowLogsCallback = Byte.MaxValue - 1;
		public const byte DisposeCallback = Byte.MaxValue;
		
		public static class Common {
			//
			public static readonly KeyboardButtonCallback Dispose_Button = new KeyboardButtonCallback {
				text = "\u274c Dispose",
				data = [DisposeCallback]
			};
			
			//
			public const byte Url_Login_Password = 31;
			public const byte Email_Login_Password = 32;
			public const byte Cookies = 33;
		}
		
		public static class Menu {
			public const byte GamesCallback = 20;
			public const byte ServicesCallback = 30;
			public const byte CommonCallback = 40;
		}
		
		public static class Games {
			// TODO
		}
		
		public static class Services {
			public const string Discord = "/discord";
			public const byte DiscordCallback = 31;
			public const string Twitch = "/twitch";
			public const byte TwitchCallback = 32;
		}

		public static BotCommand[] GenerateCommands() {
			return [
				new() {
					command = Start[1..],
					description = "Start command"
				},
				new() {
					command = Services.Discord[1..],
					description = "Use discord tokens parser"
				},
				new() {
					command = Services.Twitch[1..],
					description = "Use twitch tokens parser"
				}
			];
		}
	}
}