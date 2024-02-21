using TelegramBot.Models;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Main;

public class ServicesCommand(Client client) : ICommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user)
    {
        return client.Messages_SendMessage(
            user,
            "Available Services",
            Random.Shared.NextInt64(),
            reply_markup: new ReplyKeyboardMarkup
            {
                rows = new []
                {
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Twitch"
                            },
                            new KeyboardButton
                            {
                                text = "Telegram"
                            }
                        }
                    },
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Back"
                            }
                        }
                    }
                },
                flags = ReplyKeyboardMarkup.Flags.resize
            });
    }
}