using TL;

namespace TelegramBot.Models;

public interface ICallbackCommand
{
    public Task Invoke(UpdateBotCallbackQuery update, User user);
}