namespace Core.Models.Configs;

public class ParsingConfig
{
    public Dictionary<string, string[]> Accounts { get; init; } = new();
    public List<Cookie> Cookies { get; init; } = [];
    public List<string> Links { get; init; } = [];
}