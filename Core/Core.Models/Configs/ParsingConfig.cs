namespace Core.Models.Configs;

public class ParsingConfig
{
    public Dictionary<string, string[]> Accounts { get; init; } = new();
    public List<CookieDomain> Cookies { get; init; } = [];
    public List<string> Requests { get; init; } = [];
}