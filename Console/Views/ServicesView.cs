using Console.Extensions;
using Console.Models;
using Console.Models.Abstractions;
using Console.Models.Attributes;
using Console.Models.Views;
using Core.Parsers.Services;

namespace Console.Views;

public class ServicesView(IRoot root, DataService data, Settings settings)
    : ArgsView(root)
{
    [Command]
    public Task Twitch() => data.SaveAsync(settings.Path[(settings.Path.LastIndexOf('\\') + 1)..], settings.Path.TwitchFromLogs());

    [Command]
    public async Task Discord()
    {
        var tokens = settings.Path.DiscordByLogs().Distinct().ToArray();
        
        await data.SaveAsync("tokens", tokens);

        _ExitWait();
    }
}