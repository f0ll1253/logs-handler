using Core.Models.Abstractions;

namespace Core.Models.Realizations;

public class PasswordField : IPasswordField
{
    public string Url { get; init; } = "";
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public string Application { get; init; } = "";
}