namespace Core.Models.Abstractions;

public interface IPasswordField
{
    public string Url { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
    public string Application { get; init; }
}