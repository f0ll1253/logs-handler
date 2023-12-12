using System.Text;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Extensions;

public static class TelegramClientExtensions
{
    public static Task SendMessageAvailableLogs(this Client client,
        InputPeer peer,
        DataService data,
        Random random,
        string message)
    {
        return client.Messages_SendMessage(
            peer,
            message,
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

    public static Task SendCallbackAvailableLogs(this Client client,
        InputPeer peer,
        DataService data,
        int messageid,
        byte[] updatebytes)
    {
        var buttons = data.LogsToButtons(updatebytes[0]);
            
        return client.Messages_EditMessage(
            peer,
            messageid,
            reply_markup: new ReplyInlineMarkup
            {
                rows = buttons
                    .Append(new KeyboardButtonRow
                    {
                        buttons = new[]
                        {
                            updatebytes[0] == 0 ? null : new KeyboardButtonCallback
                            {
                                text = "\u25c0\ufe0f",
                                data = new[] { (byte) (updatebytes[0] - 1) }
                            },
                            buttons.Count() == 5 ? new KeyboardButtonCallback
                            {
                                text = "\u25b6\ufe0f",
                                data = new[] { (byte) (updatebytes[0] + 1) }
                            } : null
                        }.Where(x => x is not null).ToArray()
                    })
                    .ToArray()
            });
    }
    
    private static IEnumerable<KeyboardButtonRow> LogsToButtons(this DataService data, int start = 0, int count = 5) 
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