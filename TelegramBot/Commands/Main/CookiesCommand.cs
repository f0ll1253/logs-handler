using System.Text.RegularExpressions;
using Core.Models.Configs;
using Core.Parsers;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Main;

public class CookiesCommand(Client client, ParsingConfig cfgParse, DataService data) : ICommand, ICallbackCommand
{
    private static readonly Regex _password = new(@"(P|p)?ass(word)?.?[a-zA-Z0-9]*?\s?(:|-)?\s?(.+)($|\n)");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (await client.SendCallbackAvailableLogsOrGetPath(user, data, update.msg_id, update.data) is not
            { } logsname) return;

        await ProcessArchive(user, logsname, null);
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

        await ProcessArchive(user, filepath, GetPassword(((Message)update.message).message));
    }

    private async Task ProcessArchive(InputPeer peer, string logsname, string? password)
    {
        if (data.GetLogsPath(logsname) is not { } zippath ||
            await data.ExtractFilesAsync(peer, zippath, password) is not { } dir)
        {
            dir = data.GetExtractedPath(logsname);

            if (!Directory.Exists(dir)) return;
        }

        await ParseCookies(peer, logsname, dir);
    }

    private async Task ParseCookies(InputPeer peer, string filename, string dir)
    {
        await client.Messages_SendMessage(peer, $"Start parsing cookies from {filename}", Random.Shared.NextInt64());

        foreach ((var cookie, var cookies) in cfgParse.Cookies.CookiesFromLogs(dir))
        {
            string path = await data.SaveZipAsync(
                dir[(dir.LastIndexOf('\\') + 1)..],
                "cookies",
                cookie.Domains.First(),
                cookies.ToArray(),
                "Cookies");

            await data.SendFileAsync(peer, path);
        }

        await client.Messages_SendMessage(peer, $"{filename} successfully processed", Random.Shared.NextInt64());
    }

    private string? GetPassword(string text)
    {
        var password = _password.Match(text).Groups[4];

        if (!password.Success || password.Value.ToLower() is "none" or "\u2796") return null;

        return password.Value;
    }
}