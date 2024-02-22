namespace Core.Models;

public record Account(
    string Username,
    string Password,
    string Url = "",
    string Log = ""
) : IDisposable
{
    public void Dispose() =>
        GC.SuppressFinalize(this);

    public override string ToString() =>
        $"{Username}:{Password}:{Url}";

    public string ToStringShort() =>
        $"{Username}:{Password}";
}