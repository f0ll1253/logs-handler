namespace Console.Models;

public class ParsingConfig
{
    public Dictionary<string, string[]> Accounts { get; init; }
    public List<string> Cookies { get; init; }
    public List<string> Links { get; init; }
}