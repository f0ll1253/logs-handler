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
    private readonly SaverService _save;
    private readonly Settings _settings;
    
    public ServicesView(IRoot root, SaverService save, Settings settings) : base(root)
    {
        _save = save;
        _settings = settings;
    }
    
    [Command]
    public async Task Discord()
    {
        StreamWriter invalid = await _save.CreateStreamAsync("invalid"), valid = await _save.CreateStreamAsync("valid");
        var tokens = _settings.Path.DiscordByLogs().Distinct().ToArray();
        
        await _save.SaveAsync("tokens", tokens);

        if (invalid is null || valid is null)
        {
            _ExitWait();
            return;
        }
        
        async Task WriteInvalid(string token)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(token);
            System.Console.ForegroundColor = ConsoleColor.White;
                    
            await invalid.WriteLineAsync(token);
        }
        
        async Task WriteValid(string token, DiscordAccount account)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine(token);
            System.Console.ForegroundColor = ConsoleColor.White;
            
            await valid.WriteLineAsync(token);
        }

        await Parallel.ForEachAsync(tokens,
            async (token, cancel) =>
            {
                if (cancel.IsCancellationRequested) return;

                bool? check;
                
                do
                {
                    check = await DiscordChecker.TryLoginAsync(token);
                } while (check is null);

                if (check is false) await WriteInvalid(token);
                else await WriteValid(token, await DiscordChecker.GetInfoAsync(token));
            });

        invalid.Close();
        valid.Close();
        
        _ExitWait();
    }

    [Command]
    public async Task IGV()
    {
        System.Console.Write("File with accounts: ");
        var filepath = System.Console.ReadLine()?.Replace("\n", "");
        
        if (!File.Exists(filepath)) return;

        var lines = await File.ReadAllLinesAsync(filepath);

        System.Console.ForegroundColor = ConsoleColor.Green;
        
        foreach (var account in lines
                     .Select(x => x.Split(':'))
                     .Select(x => new Account(x[0], x[1])))
        {
            if (await IGVChecker.TryLoginAsync(account.Username, account.Password) is {})
                System.Console.WriteLine(account.ToString());
        }

        System.Console.ForegroundColor = ConsoleColor.White;
        
        _ExitWait();
    }
}