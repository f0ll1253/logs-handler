namespace TelegramBot.Models.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public required string Command { get; set; }
}