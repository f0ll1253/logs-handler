namespace Server.Models.Abstract;

public interface ICredentialsMultipleParser<T> : IMultipleParser<T>
    where T : Credentials
{
}