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
    
    public Task Invoke(UpdateNewMessage update, User user)
    {
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

        //  extracting
        if (await data.ExtractFilesAsync(user, filepath, GetPassword(((Message) update.message).message)) is not {} dir) return;
        
        // parsing
        var filename = filepath[(filepath.LastIndexOf('\\') + 1)..filepath.LastIndexOf('.')];
        
        await client.Messages_SendMessage(user, $"Start parsing cookies from {filename}", random.NextInt64());
        
        foreach (var (domain, cookies) in cfgParse.Cookies.CookiesFromLogs(dir))
        {
            var path = await data.SaveZipAsync(
                dir[(dir.LastIndexOf('\\')+1)..],
                "cookies",
                domain,
                cookies.ToArray(),
                name: "Cookies");

            await data.SendFileAsync(user, path);
        }
        
        await client.Messages_SendMessage(user, $"{filename} successfully processed", random.NextInt64());
    }

    private static readonly Regex _password = new(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)");
    private string? GetPassword(string text)
    {
        var password = _password.Match(text).Groups[4].Value;

        if (password.ToLower() is "none" or "\u2796") return null;

        return password;
    }
}