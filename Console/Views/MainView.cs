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
    private readonly SaverService _save;
    
    public MainView(IRoot root, Settings settings, IConfiguration config, SaverService save) : base(root)
    {
        _settings = settings;
        _config = config;
        _save = save;
    }

    [Command, Redirect]
    public Task Checkers()
    {
        Root.PushRedirect<CheckersView>();
        return Task.CompletedTask;
    }
    
    [Command]
    public async Task Wallets()
    {
        var checker = new MetamaskChecker(_config["Web3:Eth"]!, _config["Web3:Bsc"]!);
        var wallets = new MetamaskParser().ByLogs(_settings.Path);

        await _save.SaveAsync("mnemonics", wallets.Select(x => x?.Mnemonic).Distinct());
        
        foreach (var wallet in wallets)
        {
            if (wallet is not {Mnemonic:not null, Password:not null} ||
                wallet.Mnemonic.Split(' ').Length != 12) continue;
            
            System.Console.WriteLine(wallet.Mnemonic);
            
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
    public async Task Discord()
    {
        await _save.SaveAsync("tokens.txt", _settings.Path.DiscordByLogs().Distinct());

        _ExitWait();
    }

    [Command]
    public async Task Links()
    {
        var links = File.ReadAllLines("links.txt").Select(x => x.Trim());
        
        await _save.SaveAsync("links.txt", links.LinksFromLogs(_settings.Path).Select(x => x.ToString()));

        _ExitWait();
    }

    [Command]
    public async Task Cookies()
    {
        var domains = File.ReadAllLines("cookies.txt").Select(x => x.Trim());

        foreach (var (domain, cookies) in domains.CookiesFromLogs(_settings.Path)
                     .ToDictionary(
                         x => x.Key,
                         x => x.Value.Where(cookies => cookies.Any()).ToArray()
                     )
                )
        {
            for (var i = 0; i < cookies.Length; i++)
            {
                await _save.SaveAsync($"cookies{i}", cookies[i], subpath: domain);
            }
        }


        _ExitWait();
    }

    [Command]
    public async Task Accounts()
    {
        var domains = File.ReadAllLines("accounts.txt").Select(x => x.Trim());

        foreach (var (domain, accounts) in domains.AccountsFromLogs(_settings.Path)
                     .ToDictionary(
                         x => x.Key,
                         x => x.Value.DistinctBy(acc => acc.Username + acc.Password)
                     )
                     .ToDictionary(
                         x => x.Key,
                         x => x.Value.Select(acc => acc.ToStringShort())
                     )
                )
        {
            await _save.SaveAsync(domain, accounts);
        }

        _ExitWait();
    }

    [Command]
    public Task Steam_maFiles()
    {
        foreach (var log in Directory.GetDirectories(_settings.Path))
        {
            if (!Directory.Exists(Path.Combine(log, "Steam"))) continue;
            
            var files = Directory.GetFiles(Path.Combine(log, "Steam"), ".maFile");
            
            if (!files.Any()) continue;
            
            System.Console.WriteLine(new FileInfo(files.First()).Directory?.FullName);
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