using TL;

namespace Bot.Telegram.Commands {
	public static class Keys {
		public const string Start = "/start";
		public const byte StartCallback = 0;

		public static BotCommand[] GenerateCommands() {
			return [
				new() {
					command = Start[1..],
					description = "Start command"
				}
			];
		}
	}
}