using Core.Parsers.Services;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TwitchServiceCommand(Client client, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    // ◀️▶️
    public Task Invoke(UpdateNewMessage update, User user) => client.SendMessageAvailableLogs(user, data, "Twitch");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not {} logsname) return;
        
        var logspath = data.GetExtractedPath(logsname);
        
        await client.EditMessage(user, update.msg_id, $"Telegram\nParsing from {logsname}");
        
        var filepath = await data.SaveAsync(
            logspath[(logspath.LastIndexOf('\\') + 1)..],
            logspath.TwitchFromLogs(),
            "Twitch");

        await data.SendFileAsync(user, filepath);
    }
}