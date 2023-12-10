using Console.Extensions;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Checkers;
using Core.Checkers.Crypto;
using Core.Parsers.Services;

namespace Console.Views;

public class ServicesView : ArgsView
{
    private readonly DataService _data;
    private readonly Settings _settings;
    
    // checkers
    private readonly DiscordChecker _discord;
    private readonly IGVChecker _igv;
    private readonly CatmineChecker _catmine;

    public ServicesView(
        IRoot root, 
        DataService data, 
        Settings settings, 
        DiscordChecker discord, 
        IGVChecker igv, 
        CatmineChecker catmine) : base(root)
    {
        _data = data;
        _settings = settings;
        _discord = discord;
        _igv = igv;
        _catmine = catmine;
    }
    
    [Command]
    public async Task Discord()
    {
        StreamWriter? invalid = await _data.CreateWriterAsync("invalid"), valid = await _data.CreateWriterAsync("valid");
        var tokens = _settings.Path.DiscordByLogs().Distinct().ToArray();
        
        await _data.SaveAsync("tokens", tokens);

        if (invalid is null || valid is null)
        {
            _ExitWait();
            return;
        }
        
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
                    check = await _discord.TryLoginAsync(token);
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
        await foreach (var account in _data.ReadAccountsAsync("igv", name: "Accounts"))
        {
            if (await _igv.TryLoginAsync(account.Username, account.Password) is {})
                System.Console.Out.WriteValidLine(account.ToStringShort());
            else
                System.Console.Out.WriteInvalidLine(account.ToStringShort());
        }
        
        _ExitWait();
    }

    [Command]
    public async Task Catmine()
    {
        await foreach (var account in _data.ReadAccountsAsync("catmine", name: "Accounts"))
        {
            if (await _catmine.TryLoginAsync(account.Username, account.Password))
                System.Console.Out.WriteValidLine(account.ToStringShort());
            else
                System.Console.Out.WriteInvalidLine(account.ToStringShort());
        }
        
        _ExitWait();
    }
}