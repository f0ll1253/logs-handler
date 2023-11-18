namespace Console.Models;

public record Account(
    string Url,
    string Username,
    string Password
)
{
    public override string ToString() => $"{Username}:{Password}:{Url}";
}