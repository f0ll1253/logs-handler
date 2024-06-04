namespace Bot.Bot.Abstractions {
	public interface IUpdateHandler {
		Task HandleUpdateAsync(Update update, Dictionary<long, User> users);
		Task HandleErrorAsync(Update update, Dictionary<long, User> users, Exception exception);
	}
}