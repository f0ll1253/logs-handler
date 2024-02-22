using TL;

namespace TelegramBot.Models;

public interface ICommand
{
    public bool AuthorizedOnly { get; }

    public Task Invoke(UpdateNewMessage update, User user);
}