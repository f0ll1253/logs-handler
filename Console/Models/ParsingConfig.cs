namespace Console.Models;

public class ParsingConfig
{
    public Dictionary<string, string[]> Accounts { get; init; } = new();
    public List<string> Cookies { get; init; } = new();
    public List<string> Links { get; init; } = new();
}