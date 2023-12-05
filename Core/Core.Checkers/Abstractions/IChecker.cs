namespace Core.Checkers.Abstractions;

public interface ICredentialsChecker<TCheckResult>
{
    Task<TCheckResult> TryLoginAsync(string login, string password);
}

public interface ITokenChecker<TCheckerResult>
{
    Task<TCheckerResult> TryLoginAsync(string token);
}