namespace Core.Models;

public class Cookie
{
    public List<string> Domains { get; init; } = [];
    public List<string> Require { get; init; } = [];
    public bool IsFull { get; init; } = false;
    public bool OneFile { get; init; } = false;
}