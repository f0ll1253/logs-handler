using Core.Models.Configs;
using Core.Parsers;
using Microsoft.EntityFrameworkCore;
using TelegramBot.Data;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

using User = TL.User;

namespace TelegramBot.Commands.Main;

public class CookiesCommand(Client client, AppDbContext context, ParsingConfig cfgParse, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) =>
        client.SendAvailableLogs(user, "Cookies\nSelect logs", data);

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        if (await data.TrySendUploadedAsync(user, logsname, "Cookies"))
            return;

        await ProcessArchive(update.msg_id, user, logsname);
    }

    private async Task ProcessArchive(int messageId, InputPeer peer, string logsname)
    {
        await client.EditMessage(peer, messageId, $"Cookies\nStart parsing cookies from {logsname}");

        await data.SendFilesAsync(peer,  await ParseCookiesAsync(logsname), logsname, "Cookies");
        
        await client.EditMessage(peer, messageId, $"Cookies\n{logsname} successfully processed");
    }

    private Task<IEnumerable<string>> ParseCookiesAsync(string logsname) => cfgParse.Cookies.CookiesFromLogs(data.GetExtractedPath(logsname), data.GetServicePath(logsname, "Cookies"));
}