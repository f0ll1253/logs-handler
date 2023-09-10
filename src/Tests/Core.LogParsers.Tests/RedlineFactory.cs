using Core.Models;
using Core.Models.Logs;
using Core.Models.Logs.Abstractions;

namespace Core.LogParsers.Tests;

public class Tests
{
    private readonly RedlineFactory _factory = new();
    private readonly string _path = Path.Combine(Environment.CurrentDirectory, "Data", "redline");
    private readonly List<PasswordField> _passwords = new() {
        new()
        {
            Url = "https://account.live.com/ResetPassword.aspx",
            UserName = "mattosrogerio42@gmail.com",
            Password = "10021975RMinformatica@",
            Application = "Google_[Chrome]_Default"
        },
        new()
        {
            Url = "https://polygonscan.com/login",
            UserName = "Rminformatica2023",
            Password = "*$r@LDPF&r",
            Application = "Google_[Chrome]_Default"
        },
        new()
        {
            Url = "https://firefaucet.win/reset/924546/a26b40af4b54bcea0ffc89033171090c56ea3ff148aa63",
            UserName = "rminformatica",
            Password = "10021975RMinformatica",
            Application = "Google_[Chrome]_Default"
        },
        new()
        {
            Url = "https://us1.badoo.com/forgot",
            UserName = "rogerio.mattos_inf_2010@hotmail.com",
            Password = "10021975RMinformatica",
            Application = "Google_[Chrome]_Default"
        },
        new()
        {
            Url = "https://www.superpay.me/members/login.php",
            UserName = "RMinformatica2023",
            Password = "sjONCRFE",
            Application = "Google_[Chrome]_Default"
        },
    };
    
    [Test]
    public async Task Test_Create()
    {
        var log = await _factory.Create(_path);

        await ComparePasswords(log.Passwords);
        
        Assert.Multiple(() =>
        {
            Assert.That(log.HasWallets, Is.True);
            Assert.That(log.HasSteam, Is.False);
            Assert.That(log.HasTelegram, Is.False);
        });
    }

    [Test]
    public async Task Test_ParsePasswords()
    {
        await ComparePasswords(_factory.ParsePasswords(Path.Combine(_path, "Passwords.txt")));
    }

    [Test]
    public void Test_TelegramExists()
    {
        Assert.That(_factory.TelegramExists(_path), Is.False);
    }

    [Test]
    public void Test_WalletsExists()
    {
        Assert.That(_factory.WalletsExists(_path), Is.True);
    }

    [Test]
    public void Test_SteamExists()
    {
        Assert.That(_factory.SteamExists(_path), Is.False);
    }

    private async Task ComparePasswords(IAsyncEnumerable<IPasswordField> log)
    {
        var list = await log.ToListAsync();
        
        Assert.That(list, Has.Count.EqualTo(_passwords.Count));
        
        for (int i = 0; i < _passwords.Count; i++)
        {
            var logPass = list[i];
            var myPass = _passwords[i];
            
            Assert.Multiple(() =>
            {
                Assert.That(logPass.Url, Is.EqualTo(myPass.Url));
                Assert.That(logPass.UserName, Is.EqualTo(myPass.UserName));
                Assert.That(logPass.Password, Is.EqualTo(myPass.Password));
                Assert.That(logPass.Application, Is.EqualTo(myPass.Application));
            });
        }
    }
}