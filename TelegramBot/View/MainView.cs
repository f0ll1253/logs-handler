using System.Text.RegularExpressions;
using Core.Models.Configs;
using Core.Parsers;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers.Zip;
using TelegramBot.Models;
using TelegramBot.Models.Attributes;
using TL;
using WTelegram;

namespace TelegramBot.View;

public class MainView(Client client, ParsingConfig cfgParse) : CommandsView
{
    [Command("Cookies")]
    public Task Cookies(UpdateNewMessage update)
    {
        App.Wait.Add(update.message.Peer.ID, HandleCookiesAsync);
        
        return client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Send zip/rar file with logs", new Random().NextInt64());
    }

    private async Task HandleCookiesAsync(UpdateNewMessage update)
    {
        string filepath;
        
        try
        {
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], $"Downloading file", new Random().NextInt64());
            filepath = await DownloadFileAsync(update);
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Files was successfully downloaded", new Random().NextInt64());
        }
        catch (Exception e)
        {
            await client.Messages_SendMessage(App.Users[update.message.Peer.ID], e.Message, new Random().NextInt64());
            return;
        }

        var message = (Message) update.message;
        await ExtractLogsAsync(filepath, GetPassword(message.message));
        await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Start extracting logs from archive", new Random().NextInt64());
        
        foreach (var (domain, cookies) in  cfgParse.Cookies.CookiesFromLogs(GetFilePath(filepath)))
        {
            using var outzip = ZipArchive.Create();

            for (int i = 0; i < cookies.Count(); i++)
            {
                var memory = new MemoryStream();
                var writer = new StreamWriter(memory);

                foreach (var cookie in cookies.ElementAt(i)) await writer.WriteLineAsync(cookie);

                outzip.AddEntry($"{domain}{i}.txt", memory, true);
            }

            var savefile = new FileInfo($"Cookies/{filepath.Split('.')[0]}/{domain}.zip");
            savefile.Directory?.Create();
            savefile.Create().Close();
            
            outzip.SaveTo(savefile.FullName, new ZipWriterOptions(CompressionType.None));
            await SendFileAsync(App.Users[update.message.Peer.ID], savefile.FullName);
        }
        
        await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Logs successfully processed", new Random().NextInt64());
    }

    private async Task SendFileAsync(InputPeer peer, string filepath)
    {
        var uploaded = await client.UploadFileAsync(filepath);

        await client.Messages_SendMedia(peer,
            new InputMediaUploadedDocument(
                uploaded,
                ""),
            "",
            new Random().NextInt64());
    }

    private string GetFilePath(string filepath)
    {
        var dir = filepath.Split('.')[0];

        return Directory.Exists(Path.Combine(dir, dir)) ? Path.Combine(dir, dir) : dir;
    }

    private static readonly Regex _password = new("(P|p)ass(word)?(:)?.([a-zA-z0-1].*?)($| )");
    private string GetPassword(string text)
    {
        var match = _password.Match(text);

        return match.Groups[4].Value;
    }

    private Task ExtractLogsAsync(string filepath, string password)
    {
        if (Directory.Exists(filepath.Split('.')[0])) return Task.CompletedTask;
        
        var options = new ReaderOptions { Password = string.IsNullOrEmpty(password) ? null : password, };
        
        using IArchive zip = filepath.Split('.')[1] == "zip"
            ? ZipArchive.Open(filepath, options)
            : RarArchive.Open(filepath, options);

        var dir = filepath.Split('.')[0];
        Directory.CreateDirectory(dir);
        zip.ExtractToDirectory(dir);
        
        return Task.CompletedTask;
    }

    private async Task<string> DownloadFileAsync(UpdateNewMessage update)
    {
        var message = (Message) update.message;
        if (!message.flags.HasFlag(Message.Flags.has_media)) throw new Exception("Error zip/rar archive not found");

        var media = (MessageMediaDocument) message.media;
        var document = (Document) media.document;
        if (document.Filename.Split('.').LastOrDefault() is not ("zip" or "rar")) throw new Exception("Error zip/rar archive not found");

        if (File.Exists(document.Filename)) return document.Filename;
        
        await using var file = new FileStream(document.Filename, FileMode.Create);
        await client.DownloadFileAsync(document, file);

        return document.Filename;
    }
}