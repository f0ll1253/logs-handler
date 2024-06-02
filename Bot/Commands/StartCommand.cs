using Bot.Models.Abstractions;

namespace Bot.Commands;

[RegisterTransient(ServiceKey = "/start", ServiceType = typeof(ICommand<UpdateNewMessage>), Tags = "User")]
public class StartCommand(Client client) : ICommand<UpdateNewMessage> {
    public Task ExecuteAsync(UpdateNewMessage message, User user) {
        return client.Messages_SendMessage(
            user,
            $"Hello, {user.username}!",
            Random.Shared.NextInt64()
        );
    }
}