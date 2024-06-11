namespace Bot.Models {
	public static class Constants {
		public static string Directory_Base => AppDomain.CurrentDomain.BaseDirectory;

		// Logs
		public static string Directory_Data => Path.Combine(Constants.Directory_Base, "Data"); // Data/
		public static string Directory_Downloaded => Path.Combine(Constants.Directory_Data, "Downloaded"); // Data/Downloads
		public static string Directory_Extracted => Path.Combine(Constants.Directory_Data, "Extracted"); // Data/Extracted

		// WTelegramClient
		public static string Directory_Session => Path.Combine(Constants.Directory_Base, "Session");
	}
}