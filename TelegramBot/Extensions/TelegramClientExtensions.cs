using System.Text;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Extensions;

public static class TelegramClientExtensions
{
    public static async Task<Message> FindMessage(this Client client, int id) =>
        (Message)(await client.Messages_GetMessages(id)).Messages.First();

    public static async Task EditMessage(this Client client, InputPeer peer, int id, string message,
        ReplyMarkup? reply_markup = null) =>
        await client.Messages_EditMessage(peer, id, message,
            reply_markup: reply_markup ?? (await client.FindMessage(id)).reply_markup);

    public static Task SendMessageAvailableLogs(this Client client,
        InputPeer peer,
        string message,
        DataService data,
        IEnumerable<KeyboardButtonRow>? reply_markup = null)
    {
        var logs = data.AvailableLogs(count: 5);

        return client.Messages_SendMessage(
            peer,
            message,
            Random.Shared.NextInt64(),
            reply_markup: new ReplyInlineMarkup
            {
                rows = logs
                       .Select(x => new KeyboardButtonRow
                       {
                           buttons =
                           [
                               new KeyboardButtonCallback
                               {
                                   text = x,
                                   data = Encoding.UTF8.GetBytes(x)
                               }
                           ]
                       })
                       .Append(new KeyboardButtonRow
                       {
                           buttons =
                           [
                               logs.Count() == 5
                                   ? new KeyboardButtonCallback
                                   {
                                       text = "\u25b6\ufe0f",
                                       data = [1]
                                   }
                                   : null
                           ]
                       })
                       .Union(reply_markup ?? ArraySegment<KeyboardButtonRow>.Empty)
                       .ToArray()
            });
    }

    public static async Task<string?> SendCallbackAvailableLogsOrGetPath(this Client client,
        InputPeer peer,
        DataService data,
        int messageid,
        byte[] updatebytes)
    {
        if (updatebytes.Length != 1) return Encoding.UTF8.GetString(updatebytes);

        var buttons = data.LogsToButtons(updatebytes[0]);
        var markup = (ReplyInlineMarkup)(await client.FindMessage(messageid)).reply_markup;

        await client.Messages_EditMessage(
            peer,
            messageid,
            reply_markup: new ReplyInlineMarkup
            {
                rows = buttons
                       .Append(new KeyboardButtonRow
                       {
                           buttons = new[]
                                     {
                                         updatebytes[0] == 0
                                             ? null
                                             : new KeyboardButtonCallback
                                             {
                                                 text = "\u25c0\ufe0f",
                                                 data = [(byte)(updatebytes[0] - 1)]
                                             },
                                         buttons.Count() == 5
                                             ? new KeyboardButtonCallback
                                             {
                                                 text = "\u25b6\ufe0f",
                                                 data = [(byte)(updatebytes[0] + 1)]
                                             }
                                             : null
                                     }
                                     .Where(x => x is { })
                                     .ToArray()
                       })
                       .Union(markup.rows[(buttons.Count() + 1)..])
                       .ToArray()
            });

        return null;
    }

    private static IEnumerable<KeyboardButtonRow> LogsToButtons(this DataService data, int start = 0, int count = 5) =>
        data.AvailableLogs(start, count).Select(x => new KeyboardButtonRow
        {
            buttons =
            [
                new KeyboardButtonCallback
                {
                    text = x,
                    data = Encoding.UTF8.GetBytes(x)
                }
            ]
        });
}