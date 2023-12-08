namespace TelegramBot.Models.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}