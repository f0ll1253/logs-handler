using System.Text.RegularExpressions;
using Core.Models.Configs;
using Core.Parsers;
using TelegramBot.Models;
using TelegramBot.Models.Attributes;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.View;

public class MainView(Client client, ParsingConfig cfgParse, DataService data) : CommandsView
{
    [Command("Cookies")]
    public Task Cookies(UpdateNewMessage update)
    {
        App.Wait.Add(update.message.Peer.ID, HandleCookiesAsync);
        
        return client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Send zip/rar file with logs", new Random().NextInt64());
    }

    private async Task HandleCookiesAsync(UpdateNewMessage update)
    {
        // download
        var filepath = await data.DownloadFileFromMessageAsync(App.Users[update.message.Peer.ID], update);
        
        if (filepath is null) return;

        //  extracting
        if (await data.ExtractFilesAsync(App.Users[update.message.Peer.ID], filepath, GetPassword(((Message) update.message).message)) is not {} dir) return;
        
        // parsing
        await client.Messages_SendMessage(App.Users[update.message.Peer.ID], "Start parsing cookies from logs", new Random().NextInt64());
        
        foreach (var (domain, cookies) in cfgParse.Cookies.CookiesFromLogs(dir))
        {
            var path = await data.SaveZipAsync(
                dir[(dir.LastIndexOf('\\')+1)..],
                "cookies",
                domain,
                cookies.ToArray(),
                name: "Cookies");

            await SendFileAsync(App.Users[update.message.Peer.ID], path);
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

    private static readonly Regex _password = new(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)");
    private string? GetPassword(string text)
    {
        var password = _password.Match(text).Groups[4].Value;

        if (password.ToLower() is "none" or "\u2796") return null;

        return password;
    }
}