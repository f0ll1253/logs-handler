using System.Text.RegularExpressions;
using Core.Models.Configs;
using Core.Parsers;
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
    private static readonly Regex _password = new(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        await ProcessArchive(update.msg_id, user, logsname, null);
    }

    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user)
    {
        App.Hooks.Add(update.message.Peer.ID, HandleCookiesAsync);

        return client.SendMessageAvailableLogs(
            user,
            data,
            "Cookies\nSend zip/rar file with logs or select downloaded cookies",
            new[]
            {
                new KeyboardButtonRow
                {
                    buttons =
                    [
                        new KeyboardButtonCallback
                        {
                            text = "Cancel",
                            data = "Cancel"u8.ToArray()
                        }
                    ]
                }
            });
    }

    private async Task HandleCookiesAsync(UpdateNewMessage update, User user)
    {
        var msg = update.message as Message;

        if (msg?.message is not { Length: > 0 } filepath)
        {
            filepath = await data.DownloadFileFromMessageAsync(user, update);

            if (filepath is null) return;
        }

        string? password =
            _password.Match(((Message)update.message).message).Groups[4]
                is { Success: true, Value: not "none" and "\u2796" } result
                ? result.Value
                : null;
        
        await ProcessArchive(update.message.ID, user, filepath, password);
    }

    private async Task ProcessArchive(int messageId, InputPeer peer, string logsname, string? password)
    {
        await client.EditMessage(peer, messageId, $"Cookies\nStart parsing cookies from {logsname}");
        
        if (data.GetLogsPath(logsname) is not { } zippath ||
            await data.ExtractFilesAsync(peer, zippath, password) is not { } dir)
        {
            dir = data.GetExtractedPath(logsname);

            if (!Directory.Exists(dir)) return;
        }

        var files = await ParseCookies(logsname, dir);
        
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

    private async Task<IEnumerable<InputFileBase>> ParseCookies(string filename, string logs)
    {
        var files = new List<InputFileBase>();
        
        foreach ((var cookie, var cookies) in cfgParse.Cookies.CookiesFromLogs(logs))
        {
            string path = await data.SaveZipAsync(
                new DirectoryInfo(logs).Name,
                "cookies",
                cookie.Domains.First(),
                cookies.ToArray(),
                "Cookies");
            
            files.Add(await client.UploadFileAsync(path));
        }

        return files;
    }
    
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
            Type = new FileInfo(x.Filename).Name,
            Category = "Cookies",
            LogsName = logsname
        }));

        await context.SaveChangesAsync();
    }
}