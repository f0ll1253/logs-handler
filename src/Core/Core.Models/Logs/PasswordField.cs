using Core.Models.Logs.Abstractions;

namespace Core.Models.Logs;

public class PasswordField : IPasswordField
{
    public string Url { get; init; } = "";
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public string Application { get; init; } = "";
}