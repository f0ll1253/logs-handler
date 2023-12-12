namespace Core.Models;

public class Cookie
{
    public List<string> Domains { get; init; } = new();
    public List<string> Require { get; init; } = new();
    public bool IsFull { get; init; } = false;
    public bool OneFile { get; init; } = false;
}