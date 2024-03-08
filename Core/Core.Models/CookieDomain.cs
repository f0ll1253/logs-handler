namespace Core.Models;

public class CookieDomain
{
    public List<string> Domains { get; init; } = [];
    public bool IsFull { get; init; } = false;
    public bool OneFile { get; init; } = false;
}