using System.Reflection;
using TelegramBot.Models.Attributes;

namespace TelegramBot.Models;

public abstract class CommandsView
{
    public IReadOnlyDictionary<string, MethodInfo> Commands { get; set; } = new Dictionary<string, MethodInfo>();

    public void Initialize()
    {
        Commands = this
            .GetType()
            .GetMethods()
            .Where(x => x.IsDefined(typeof(CommandAttribute)))
            .ToDictionary(x => x.GetCustomAttribute<CommandAttribute>()!.Command, x => x);
    }
}