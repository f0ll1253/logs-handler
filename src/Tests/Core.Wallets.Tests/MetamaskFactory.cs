using Core.Models;
using Core.Models.Logs;
using Core.Models.Wallets;

namespace Core.Wallets.Tests;

public class Tests
{
    private readonly MetamaskFactory _factory = new();

    [Test]
    public async Task Test_Create()
    {
        var logPath = Path.Combine(Environment.CurrentDirectory, "Data", "log");
        var log = new Log {
            Path = logPath,
            HasWallets = true,
            Passwords = new List<PasswordField> {
                new()
                {
                    Password = "10021975RMinformatica@"
                },
                new()
                {
                    Password = "*$r@LDPF&r"
                },
                new()
                {
                    Password = "10021975RMinformatica"
                },
                new()
                {
                    Password = "sjONCRFE"
                },
            }.ToAsyncEnumerable()
        };
        var wallets = await _factory.Create(log).ToListAsync();
        
        Assert.That(wallets, Has.Count.EqualTo(1));
        
        Assert.Multiple(() =>
        {
            Assert.That(wallets[0].Log, Is.EqualTo(Path.Combine(logPath, "Wallets", "Google_[Chrome]_Default_Metamask", "024961.log")));
            Assert.That(wallets[0].Mnemonic, Is.EqualTo("doctor save fox snack mass settle vehicle enrich daughter rigid level suggest"));
            Assert.That(wallets[0].Address, Is.EqualTo("0x05f246bbd176f1ac982023a7de0775e2f48e7663"));
        });
    }
}