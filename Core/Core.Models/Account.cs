namespace Core.Models;

public record Account(
    string Username,
    string Password,
    string Url = ""
)
{
    public override string ToString() => $"{Username}:{Password}:{Url}";
    public string ToStringShort() => $"{Username}:{Password}";
}